using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;

// Extensions in Morcatko.AspNetCore.JsonMergePatch.SystemText kept for retro-compatibility.

namespace Morcatko.AspNetCore.JsonMergePatch.SystemText
{
	public static class SystemTextMvxBuilderExtensions
	{
        public static IMvcBuilder AddSystemTextJsonMergePatch(this IMvcBuilder builder, Action<JsonMergePatchOptions> configure = null)
            => JsonMergePatch.SystemTextMvxBuilderExtensions.AddSystemTextJsonMergePatch(builder, configure);

        public static IMvcCoreBuilder AddSystemTextJsonMergePatch(this IMvcCoreBuilder builder, Action<JsonMergePatchOptions> configure = null)
            => JsonMergePatch.SystemTextMvxBuilderExtensions.AddSystemTextJsonMergePatch(builder, configure);
    }
}
