using ChatCorporaAnnotator.Data.WinFormsIntegration.AppEventArgs;
using ChatCorporaAnnotator.Data.WinFormsIntegration.Presenters.Views;
using ChatCorporaAnnotator.Data.WinFormsIntegration.Services;
using IndexEngine;
using IndexEngine.Indexes;
using IndexEngine.Paths;
using System;
using System.Linq;
using System.Windows;

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.Presenters
{
    public class TagPresenter
    {
        private readonly ITagView _tagger;
        private readonly ITagService _service;
        private readonly ITagsetView _tagset;
        private readonly IMainView _main;
        private readonly ITagFileWriter _writer;

        public TagPresenter(IMainView main, ITagView tagger, ITagService service, ITagsetView tagset, ITagFileWriter writer)
        {
            _main = main;
            _tagger = tagger;
            _service = service;
            _tagset = tagset;
            _writer = writer;

            _tagger.TagsetClick += _tagger_TagsetClick;
            _tagger.WriteToDisk += _tagger_WriteToDisk;
            _tagger.AddTag += _tagger_AddTag;
            _tagger.RemoveTag += _tagger_RemoveTag;
            _tagger.DeleteSituation += _tagger_DeleteSituation;
            _tagger.EditSituation += _tagger_EditSituation;
            _tagger.MergeSituations += _tagger_MergeSituations;
            _tagger.CrossMergeSituations += _tagger_CrossMergeSituations;
            _tagger.LoadTagset += _tagger_LoadTagset;
            _tagger.SaveTagged += _tagger_SaveTagged;
            _tagger.LoadTagged += LoadTagged;
        }

        private void _tagger_CrossMergeSituations(object sender, SituationArrayEventArgs args)
        {
            SituationIndex.GetInstance().CrossMergeItems(args.args[0].Tag, args.args[0].Id, args.args[1].Tag, args.args[0].Id);

            foreach (var id in SituationIndex.GetInstance().IndexCollection[args.args[0].Tag][args.args[0].Id])
            {
                MessageContainer.InsertTagsInDynamicMessage(id, 0);
            }

            foreach (var id in SituationIndex.GetInstance().IndexCollection[args.args[1].Tag][args.args[1].Id])
            {
                MessageContainer.InsertTagsInDynamicMessage(id, 0);
            }
        }

        private void _tagger_RemoveTag(object sender, TaggerEventArgs args)
        {
            SituationIndex.GetInstance().DeleteMessageFromSituation(args.Tag, args.Id, args.messages[0]);
            MessageContainer.Messages[args.messages[0]].Situations.Remove(args.Tag);

            if (SituationIndex.GetInstance().GetInnerValueCount(args.Tag, args.Id) == 0)
            {
                DeleteOrEditTag(args, true);
            }
        }

        private void _tagger_MergeSituations(object sender, SituationArrayEventArgs args)
        {
            DeleteOrEditTag(args.args[0], false);
        }

        private void _tagger_EditSituation(object sender, TaggerEventArgs e)
        {
            DeleteOrEditTag(e, false);
        }

        private void _tagger_DeleteSituation(object sender, TaggerEventArgs args)
        {
            DeleteOrEditTag(args, true);
        }

        private void DeleteOrEditTag(TaggerEventArgs args, bool type, int index = -1)
        {
            foreach (var id in SituationIndex.GetInstance().IndexCollection[args.Tag][args.Id])
            {
                MessageContainer.Messages[id].Situations.Remove(args.Tag);
            }

            if (type)
            {
                _tagger.UpdateSituationCount(SituationIndex.GetInstance().ItemCount);
            }
            else if (index == -1)
            {
                var tag = args.AdditionalInfo["Change"].ToString();
                var count = SituationIndex.GetInstance().GetValueCount(tag);
                var list = SituationIndex.GetInstance().IndexCollection[args.Tag][args.Id];

                SituationIndex.GetInstance().AddInnerIndexEntry(tag, count, list);

                foreach (var id in list)
                {
                    try
                    {
                        MessageContainer.Messages[id].Situations.Add(tag, count);
                    }
                    catch (ArgumentException)
                    {
                        MessageBox.Show("todo");
                    }
                }

                _tagger.AddSituationIndexItem(tag + " " + count);
            }

            SituationIndex.GetInstance().DeleteInnerIndexEntry(args.Tag, args.Id);
            _tagger.DeleteSituationIndexItem(args.Tag + " " + args.Id.ToString());

            if (args.Id < SituationIndex.GetInstance().GetValueCount(args.Tag) + 1)
            {
                for (int i = args.Id + 1; i <= SituationIndex.GetInstance().GetValueCount(args.Tag); i++)
                {
                    var list = SituationIndex.GetInstance().IndexCollection[args.Tag][i];

                    foreach (var id in list)
                    {
                        MessageContainer.Messages[id].Situations[args.Tag]--;
                    }

                    SituationIndex.GetInstance().DeleteInnerIndexEntry(args.Tag, i);
                    SituationIndex.GetInstance().AddInnerIndexEntry(args.Tag, i - 1, list);

                    _tagger.DeleteSituationIndexItem(args.Tag + " " + i.ToString());
                    _tagger.AddSituationIndexItem(args.Tag + " " + (i - 1).ToString());
                }
            }
        }

        private void LoadTagged(object sender, EventArgs e)
        {
            SituationIndex.GetInstance().ReadIndexFromDisk();
            //SituationIndexNew.GetInstance().RemakeOldIndexFile();
            //SituationIndexNew.GetInstance().RemakeFromInverted();

            foreach (var kvp in SituationIndex.GetInstance().IndexCollection)
            {
                foreach (var sit in kvp.Value)
                {
                    _tagger.AddSituationIndexItem(kvp.Key + " " + sit.Key.ToString());
                }
            }

            _service.TaggedIds = SituationIndex.GetInstance().InvertedIndex.Keys.ToList();
            _service.TaggedIds.Sort();
            _tagger.UpdateSituationCount(SituationIndex.GetInstance().ItemCount);
        }

        private void _tagger_SaveTagged(object sender, EventArgs e)
        {
            if (_main.FileLoadState)
            {
                SituationIndex.GetInstance().FlushIndexToDisk();
                TagsetIndex.GetInstance().FlushIndexToDisk();
                UserDictsIndex.GetInstance().FlushIndexToDisk();
            }
        }

        private void _tagger_LoadTagset(object sender, TaggerEventArgs e)
        {
            if (ProjectInfo.TagsetSet)
            {
                _tagger.DisplayTagset(TagsetIndex.GetInstance().IndexCollection[ProjectInfo.Tagset].Keys.ToList());
                _main.SetTagsetLabel(ProjectInfo.Tagset);
                _tagger.DisplayTagsetColors(TagsetIndex.GetInstance().IndexCollection[ProjectInfo.Tagset]);
            }
        }

        private void _tagger_AddTag(object sender, TaggerEventArgs e)
        {
            SituationIndex.GetInstance().AddInnerIndexEntry(e.Tag, e.Id, e.messages);

            foreach (var id in e.messages)
            {
                if (!MessageContainer.Messages[id].Situations.ContainsKey(e.Tag))
                    MessageContainer.Messages[id].Situations.Add(e.Tag, e.Id);
                else
                    _tagger.DisplayTagErrorMessage();
            }

            _tagger.AddSituationIndexItem(e.Tag + " " + (SituationIndex.GetInstance().GetValueCount(e.Tag) - 1));
            _tagger.UpdateSituationCount(SituationIndex.GetInstance().ItemCount);
        }

        private void _tagger_WriteToDisk(object sender, WriteEventArgs e)
        {
            _writer.OpenWriter();

            foreach (var kvp in SituationIndex.GetInstance().IndexCollection)
            {
                foreach (var pair in kvp.Value)
                {
                    _writer.WriteSituation(pair.Value, kvp.Key, pair.Key); //was this hard?
                }
            }

            _writer.CloseWriter();
        }

        private void _tagger_TagsetClick(object sender, EventArgs e)
        {
            _tagset.ShowView();
        }
    }
}
