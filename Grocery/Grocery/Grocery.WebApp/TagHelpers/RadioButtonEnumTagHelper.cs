using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

// Source: https://stackoverflow.com/questions/33680466/radio-buttons-with-individual-labels-for-enum-using-mvc6-tag-helpers

namespace Grocery.WebApp.TagHelpers
{
    /// <summary>
    /// <see cref="ITagHelper"/> implementation targeting &lt;enum-radio-button&gt; elements with an <c>asp-for</c> attribute, <c>value</c> attribute.
    /// </summary>
    [HtmlTargetElement("enum-radio-button", Attributes = RadioButtonEnumForAttributeName)]
    public class RadioButtonEnumTagHelper : TagHelper
    {
        private const string RadioButtonEnumForAttributeName = "asp-for";
        private const string RadioButtonEnumValueAttributeName = "value";

        /// <summary>
        /// Creates a new <see cref="RadioButtonEnumTagHelper"/>.
        /// </summary>
        /// <param name="generator">The <see cref="IHtmlGenerator"/>.</param>
        public RadioButtonEnumTagHelper(IHtmlGenerator generator)
        {
            Generator = generator;
        }

        /// <inheritdoc />
        public override int Order => -1000;

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        protected IHtmlGenerator Generator { get; }

        /// <summary>
        /// An expression to be evaluated against the current model.
        /// </summary>
        [HtmlAttributeName(RadioButtonEnumForAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeName(RadioButtonEnumValueAttributeName)]
        public Enum value { get; set; }

        /// <inheritdoc />
        /// <remarks>Does nothing if <see cref="For"/> is <c>null</c>.</remarks>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {

            var childContent = await output.GetChildContentAsync();
            string innerContent = childContent.GetContent();
            output.Content.AppendHtml(innerContent);

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.Add("class", "btn-group btn-group-radio");

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            var modelExplorer = For.ModelExplorer;
            var metaData = For.Metadata;

            if (metaData.EnumNamesAndValues != null)
            {
                foreach (var item in metaData.EnumNamesAndValues)
                {
                    string enum_id = $"{metaData.ContainerType.Name}_{metaData.PropertyName}_{item.Key}";

                    bool enum_ischecked = false;
                    if (value != null)
                    {
                        if (value != null && item.Key.ToString() == value.ToString())
                        {
                            enum_ischecked = true;
                        }
                    }
                    else
                    {
                        if (For.Model != null && item.Key.ToString() == For.Model.ToString())
                        {
                            enum_ischecked = true;
                        }
                    }

                    string enum_input_label_name = item.Key;
                    var enum_resourced_name = metaData.EnumGroupedDisplayNamesAndValues.Where(x => x.Value == item.Value).FirstOrDefault();
                    if (enum_resourced_name.Value != null)
                    {
                        enum_input_label_name = enum_resourced_name.Key.Name;
                    }

                    var enum_radio = Generator.GenerateRadioButton(
                        ViewContext,
                        For.ModelExplorer,
                        metaData.PropertyName,
                        item.Key,
                        false,
                        htmlAttributes: new { @id = enum_id });
                    enum_radio.Attributes.Remove("checked");
                    if (enum_ischecked)
                    {
                        enum_radio.MergeAttribute("checked", "checked");
                    }
                    output.Content.AppendHtml(enum_radio);

                    var enum_label = Generator.GenerateLabel(
                        ViewContext,
                        For.ModelExplorer,
                        For.Name,
                        enum_input_label_name,
                        htmlAttributes: new { @for = enum_id, @Class = "btn btn-default" });
                    output.Content.AppendHtml(enum_label);
                }
            }
        }
    }
}
