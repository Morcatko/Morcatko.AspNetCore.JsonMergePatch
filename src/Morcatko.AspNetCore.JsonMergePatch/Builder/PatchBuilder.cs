namespace Morcatko.AspNetCore.JsonMergePatch.Builder
{
    internal static class PatchBuilder
    {
        public static JsonMergePatchDocument<TModel> Build<TModel>(TModel original, TModel patched) where TModel : class
        {
            //Compute diff
            return null;
        }
    }
}