using ChatCorporaAnnotator.Data.WinFormsIntegration.AppEventArgs;
using ChatCorporaAnnotator.Data.WinFormsIntegration.Presenters.Views;
using ChatCorporaAnnotator.Data.WinFormsIntegration.Services;
using IndexEngine.Indexes;
using IndexEngine.Paths;
using System;
using System.IO;
using System.Linq;

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.Presenters
{
    public class TagsetPresenter
    {
        private readonly ITagsetView _tagset;
        private readonly ITagService _service;
        private readonly ITagView _main;

        public TagsetPresenter(ITagsetView tagset, ITagService service, ITagView main)
        {
            _tagset = tagset;
            _service = service;
            _main = main;

            _tagset.UpdateTagset += _tagset_UpdateTagset;
            _tagset.AddNewTagset += _tagset_AddNewTagset;
            _tagset.LoadExistingTagset += _tagset_LoadExistingTagset;
            _tagset.SetProjectTagset += _tagset_SetProjectTagset;
            _tagset.DeleteTagset += _tagset_DeleteTagset;

            _tagset.DisplayTagsetNames(TagsetIndex.GetInstance().IndexCollection.Keys.ToList());
        }

        private void _tagset_DeleteTagset(object sender, TagsetUpdateEventArgs args)
        {
            TagsetIndex.GetInstance().DeleteIndexEntry(args.Name);
        }

        private void _tagset_SetProjectTagset(object sender, TagsetUpdateEventArgs args)
        {
            if (!ProjectInfo.TagsetSet)
            {
                File.WriteAllText(ProjectInfo.TagsetPath, args.Name);
            }
            else
            {
                File.WriteAllText(ProjectInfo.TagsetPath, String.Empty);
                File.WriteAllText(ProjectInfo.TagsetPath, args.Name);
            }

            _tagset.DisplayProjectTagsetName(ProjectInfo.Tagset);
            _main.DisplayTagset(TagsetIndex.GetInstance().IndexCollection[args.Name].Keys.ToList());
            _main.DisplayTagsetColors(TagsetIndex.GetInstance().IndexCollection[args.Name]);
        }

        private void _tagset_UpdateTagset(object sender, TagsetUpdateEventArgs args)
        {
            if (args.Type == 0)
            {
                TagsetIndex.GetInstance().DeleteInnerIndexEntry(args.Name, args.Tag);
            }
            if (args.Type == 1)
            {
                TagsetIndex.GetInstance().AddInnerIndexEntry(args.Name, args.Tag);
            }
        }

        private void _tagset_LoadExistingTagset(object sender, TagsetUpdateEventArgs args)
        {
            _tagset.DisplayTagset(TagsetIndex.GetInstance().IndexCollection[args.Name]);
        }

        private void _tagset_AddNewTagset(object sender, TagsetUpdateEventArgs e)
        {
            //_service.UpdateTagsetIndex(e.Name);
            TagsetIndex.GetInstance().AddIndexEntry(e.Name, null);
            _tagset.DisplayTagsetNames(TagsetIndex.GetInstance().IndexCollection.Keys.ToList());
        }
    }
}
