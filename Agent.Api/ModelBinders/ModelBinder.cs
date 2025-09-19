// <copyright file="ModelBinder.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Api.ModelBinders;

using System.Collections;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

public class ModelBinder<T> : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        // The 'bindingContext' should only be used within this method.
        var modelType = typeof(T);
        var values = bindingContext.ActionContext.HttpContext.Request.Query;

        // Create the model instance using Activator
        var model = Activator.CreateInstance(modelType);

        // Null check after model creation
        if (model == null)
        {
            bindingContext.ModelState.AddModelError(string.Empty, $"Failed to create an instance of {modelType.Name}");
            return Task.CompletedTask;
        }

        // Bind top-level properties (simple types)
        BindProperties(modelType, model, values, bindingContext);

        // Bind deep properties (complex objects or collections)
        BindNestedProperties(modelType, model, values, bindingContext);

        // Set the result of the binding process
        bindingContext.Result = ModelBindingResult.Success(model);

        return Task.CompletedTask;
    }

    private void BindProperties(Type modelType, object model, IQueryCollection values, ModelBindingContext bindingContext)
    {
        foreach (var prop in modelType.GetProperties())
        {
            if (!prop.CanWrite || !values.ContainsKey(prop.Name))
            {
                continue;
            }

            var value = values[prop.Name].FirstOrDefault();

            if (value != null)
            {
                try
                {
                    var convertedValue = Convert.ChangeType(value, prop.PropertyType);
                    prop.SetValue(model, convertedValue);
                }
                catch (Exception ex)
                {
                    bindingContext.ModelState.AddModelError(prop.Name, $"Failed to convert {prop.Name} to {prop.PropertyType.Name}: {ex.Message}");
                }
            }
        }
    }

    private void BindNestedProperties(Type modelType, object model, IQueryCollection values, ModelBindingContext bindingContext)
    {
        foreach (var prop in modelType.GetProperties())
        {
            if (prop.PropertyType.IsSimpleType() || !values.ContainsKey(prop.Name))
            {
                continue;
            }

            var propertyValue = values[prop.Name].FirstOrDefault();

            if (string.IsNullOrEmpty(propertyValue))
            {
                if (IsNullableType(prop.PropertyType))
                {
                    prop.SetValue(model, null);
                }

                continue; // Skip empty values for nullable properties
            }

            try
            {
                if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && !propertyValue.StartsWith("{"))
                {
                    // Handle collections of complex objects
                    var listType = typeof(List<>).MakeGenericType(prop.PropertyType.GetGenericArguments());
                    var list = (IList?)Activator.CreateInstance(listType);

                    if (list == null)
                    {
                        bindingContext.ModelState.AddModelError(prop.Name, $"Failed to create a list for property {prop.Name}.");
                        continue;
                    }

                    var items = JsonConvert.DeserializeObject<List<object>>(propertyValue);

                    // Check if the deserialization is successful
                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            list.Add(item);
                        }
                    }

                    prop.SetValue(model, list);
                }
                else
                {
                    // Handle simple (non-collection) complex types
                    var nestedType = prop.PropertyType;
                    var nestedModel = Activator.CreateInstance(nestedType);

                    if (nestedModel != null)
                    {
                        BindProperties(nestedType, nestedModel, values, bindingContext);
                        prop.SetValue(model, nestedModel);
                    }
                    else
                    {
                        bindingContext.ModelState.AddModelError(prop.Name, $"Failed to create an instance of the nested model for property {prop.Name}.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any errors that occur during binding
                bindingContext.ModelState.AddModelError(prop.Name, $"Failed to bind nested property {prop.Name}: {ex.Message}");
            }
        }
    }

    private bool IsNullableType(Type type)
    {
        return Nullable.GetUnderlyingType(type) != null || !type.IsValueType;
    }
}
