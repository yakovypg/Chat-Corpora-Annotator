using ChatCorporaAnnotator.Data.Indexing;
using ChatCorporaAnnotator.Infrastructure.Enums;
using ChatCorporaAnnotator.Models.Containers;
using IndexEngine.Containers;
using IndexEngine.Data.Paths;
using IndexEngine.Indexes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatCorporaAnnotator.Services.Csv
{
    internal sealed class CsvExportService
    {
        private readonly string[] MessageExportingFields = new string[]
        {
            ProjectInfo.SenderFieldKey,
            ProjectInfo.DateFieldKey,
            ProjectInfo.TextFieldKey
        };

        private Task _exportingTask;
        private bool _stopExportingTask;
        private Dictionary<int, Tuple<string, bool>> _taggedMessagesInfo;

        public Action SuccessfulExportingAction { get; set; }
        public Action<Exception> FailedExportingAction { get; set; }
        public OperationState ExportingState { get; private set; }

        public CsvExportService()
        {
            _stopExportingTask = false;
            _taggedMessagesInfo = new Dictionary<int, Tuple<string, bool>>();

            ExportingState = OperationState.NotStarted;
        }

        public bool StartExporting(string path)
        {
            if (ExportingState == OperationState.InProcess)
                return false;

            ExportingState = OperationState.InProcess;

            int firstMessageId = IndexInteraction.GetFirstMessageId();
            int lastMessageId = IndexInteraction.GetLastMessageId();

            string[] exportingColumns = new string[]
            {
                    "Unnamed: 0",
                    ProjectInfo.SenderFieldKey,
                    ProjectInfo.DateFieldKey,
                    ProjectInfo.TextFieldKey,
                    "mark"
            };

            _exportingTask = Task.Run(delegate
            {
                var writer = new CsvWriteService(',', '"');

                try
                {
                    _taggedMessagesInfo = GetTaggedMessagesInfo();

                    writer.OpenWriter(path);
                    writer.WriteRow(exportingColumns);

                    for (int i = firstMessageId; i <= lastMessageId; ++i)
                    {
                        if (_stopExportingTask)
                        {
                            _stopExportingTask = false;
                            ExportingState = OperationState.Aborted;
                            break;
                        }

                        string[] data = GetMessageExportData(i);
                        writer.WriteRow(data);
                    }

                    writer.CloseWriter();

                    ExportingState = OperationState.Success;
                    SuccessfulExportingAction?.Invoke();
                }
                catch (Exception ex)
                {
                    writer.CloseWriter();

                    ExportingState = OperationState.Fail;
                    FailedExportingAction?.Invoke(ex);
                }
            });

            return true;
        }

        public void StopExporting()
        {
            _stopExportingTask = true;
        }

        public void StopExportingAndWait(int timeout)
        {
            StopExporting();

            if (ExportingState == OperationState.InProcess)
                _exportingTask.Wait(timeout);
        }

        private string[] GetMessageExportData(int messageId)
        {
            DynamicMessage message = IndexHelper.GetMessage(messageId);

            bool isTagged = _taggedMessagesInfo.ContainsKey(message.Id);
            bool isFirstInSituation = isTagged && _taggedMessagesInfo[message.Id].Item2;

            string tagName = isTagged ? _taggedMessagesInfo[message.Id].Item1 : null;

            MessageMarkType markType = MessageMark.GetMessageMarkType(isTagged, isFirstInSituation);
            MessageMark mark = new MessageMark(markType, tagName);

            string[] id = new string[] { message.Id.ToString() };
            string[] content = message.GetContent(MessageExportingFields);
            string[] markPresenter = new string[] { mark.ToString() };

            return id.Concat(content).Concat(markPresenter).ToArray();
        }

        private Dictionary<int, Tuple<string, bool>> GetTaggedMessagesInfo()
        {
            SituationIndex index = SituationIndex.GetInstance();

            var indexCollection = index.IndexCollection;
            var invertedIndex = index.InvertedIndex;

            var taggedMessagesInfo = new Dictionary<int, Tuple<string, bool>>();

            foreach (var kvp in invertedIndex)
            {
                if (kvp.Value.Count == 0)
                    continue;

                int messageId = kvp.Key;
                var situationData = kvp.Value.First();

                string situationHeader = situationData.Key;
                int situationId = situationData.Value;

                List<int> situationMessages = indexCollection[situationHeader][situationId].ToList();

                string markName = $"{situationHeader} {situationId}";
                bool isMessageFirstInSituation = situationMessages.Min() == messageId;

                var messageInfo = new Tuple<string, bool>(markName, isMessageFirstInSituation);
                taggedMessagesInfo.Add(messageId, messageInfo);
            }

            return taggedMessagesInfo;
        }
    }
}
