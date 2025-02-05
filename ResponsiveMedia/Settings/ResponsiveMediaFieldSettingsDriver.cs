using Etch.OrchardCore.Fields.ResponsiveMedia.Fields;
using Etch.OrchardCore.Fields.ResponsiveMedia.Utils;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Media;
using System.Threading.Tasks;

namespace Etch.OrchardCore.Fields.ResponsiveMedia.Settings
{
    public class ResponsiveMediaFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<ResponsiveMediaField>
    {
        #region Dependencies

        private readonly IMediaFileStore _mediaFileStore;
        private readonly IStringLocalizer<ResponsiveMediaFieldSettingsDriver> T;

        #endregion

        #region Constructor

        public ResponsiveMediaFieldSettingsDriver(IStringLocalizer<ResponsiveMediaFieldSettingsDriver> localizer, IMediaFileStore mediaFileStore)
        {
            T = localizer;
            _mediaFileStore = mediaFileStore;
        }

        #endregion

        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
        {
            return Initialize<ResponsiveMediaFieldSettings>("ResponsiveMediaFieldSettings_Edit", viewModel =>
            {
                var settings = partFieldDefinition.GetSettings<ResponsiveMediaFieldSettings>();
                viewModel.Required = settings.Required;
                viewModel.AllowMediaText = settings.AllowMediaText;
                viewModel.Breakpoints = settings.Breakpoints;
                viewModel.FallbackData = settings.FallbackData;
                viewModel.Hint = settings.Hint;
                viewModel.LazyLoad = settings.LazyLoad;
                viewModel.Multiple = settings.Multiple;
                viewModel.Required = viewModel.Required;
            })
            .Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition model, UpdatePartFieldEditorContext context)
        {
            var viewModel = new UpdateResponsiveMediaFieldSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(viewModel, Prefix);

            var settings = new ResponsiveMediaFieldSettings
            {
                AllowMediaText = viewModel.AllowMediaText,
                Hint = viewModel.Hint,
                LazyLoad = viewModel.LazyLoad,
                Multiple = viewModel.Multiple,
                Required = viewModel.Required,
                FallbackData = JsonConvert.SerializeObject(ResponsiveMediaUtils.ParseMedia(_mediaFileStore, viewModel.FallbackData))
            };

            try
            {
                settings.Breakpoints = viewModel.Breakpoints;
                settings.GetBreakpoints();
            }
            catch
            {
                context.Updater.ModelState.AddModelError(Prefix, T["Failed to parse breakpoints, make sure it only contains numeric values."]);
            }

            context.Builder.WithSettings(settings);

            return Edit(model, context);
        }
    }
}
