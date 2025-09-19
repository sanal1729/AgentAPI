// <copyright file="ModelBinderProvider.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Api.ModelBinders
{
    using System;
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    public class ModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            // Check if the model type is a generic type
            if (context.Metadata.ModelType.IsGenericType)
            {
                // Create an instance of the appropriate ModelBinder<> for the model type
                var modelBinderType = typeof(ModelBinder<>).MakeGenericType(context.Metadata.ModelType);

                // Use Activator to create an instance of the ModelBinder<type>
                var modelBinder = Activator.CreateInstance(modelBinderType);

                // If the modelBinder is null, we return null to avoid errors.
                return modelBinder as IModelBinder;
            }

            // Return null if the model is not generic
            return null;
        }
    }
}
