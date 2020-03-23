using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;

namespace Morcatko.AspNetCore.JsonMergePatch.SystemText
{
    [Obsolete("Moved to namespace Morcatko.AspNetCore.JsonMergePatch")]
	public static class SystemTextMvxBuilderExtensions
	{
        [Obsolete("Moved to namespace Morcatko.AspNetCore.JsonMergePatch")]
        public static IMvcBuilder AddSystemTextJsonMergePatch(this IMvcBuilder builder, Action<JsonMergePatchOptions> configure = null)
            => JsonMergePatch.SystemTextMvxBuilderExtensions.AddSystemTextJsonMergePatch(builder, configure);

        [Obsolete("Moved to namespace Morcatko.AspNetCore.JsonMergePatch")]
        public static IMvcCoreBuilder AddSystemTextJsonMergePatch(this IMvcCoreBuilder builder, Action<JsonMergePatchOptions> configure = null)
            => JsonMergePatch.SystemTextMvxBuilderExtensions.AddSystemTextJsonMergePatch(builder, configure);
    }
}
