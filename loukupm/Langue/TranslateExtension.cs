using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace loukupm.Langue
{
    [SupportedOSPlatform("IOS")]
    [SupportedOSPlatform("android")]
    [SupportedOSPlatform("windows")]
    [ContentProperty(nameof(Name))]
    public class TranslateExtension : IMarkupExtension<BindingBase>
    {
        public string? Name { get; set; }

        public string? StringFormat { get; set; }

        public BindingBase ProvideValue(IServiceProvider serviceProvider)
        {
            return new Binding
            {
                Mode = BindingMode.OneWay,
                Path = $"[{Name}]",
                Source = Langue.LocalizationResourcesManager.Instanse,
                StringFormat = StringFormat
            };
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
            => ProvideValue(serviceProvider);
    }
}
