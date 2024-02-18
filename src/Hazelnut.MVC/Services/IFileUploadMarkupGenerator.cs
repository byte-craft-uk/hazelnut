using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq.Expressions;

namespace Hazelnut.Mvc
{
    public interface IFileUploadMarkupGenerator
    {
        IHtmlContent Generate<TModel, TProperty>(IHtmlHelper html, object viewModel, Expression<Func<TModel, TProperty>> property, object htmlAttributes);
    }
}