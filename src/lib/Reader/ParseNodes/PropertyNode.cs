// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader
{
    internal class PropertyNode : ParseNode
    {
        public PropertyNode(ParsingContext context, string name, JsonNode node) : base(
            context, node)
        {
            Name = name;
            Value = Create(context, node);
        }

        /// <summary>
        /// Gets or sets the property name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value of the property node.
        /// </summary>
        public ParseNode Value { get; set; }

        /// <summary>
        /// Parses the field and applies the appropriate mapping or error handling.
        /// </summary>
        /// <typeparam name="T">The type of the parent instance.</typeparam>
        /// <param name="parentInstance">The parent instance.</param>
        /// <param name="fixedFields">Dictionary of fixed field mappings.</param>
        /// <param name="patternFields">Dictionary of pattern field mappings.</param>
        public void ParseField<T>(
            T parentInstance,
            Dictionary<string, Action<T, ParseNode>> fixedFields,
            Dictionary<Func<string, bool>, Action<T, string, ParseNode>> patternFields)
        {
            if (fixedFields.TryGetValue(Name, out var fixedFieldMap))
            {
                try
                {
                    Context.StartObject(Name);
                    fixedFieldMap(parentInstance, Value);
                }
                catch (OverlayReaderException ex)
                {
                    //TODO we're loosing the callstack here, it might be worth implementing an implicit converter or a derived class for the error
                    Context.Diagnostic.Errors.Add(new(ex.Pointer, ex.Message));
                }
                catch (OpenApiException ex)
                {
                    ex.Pointer = Context.GetLocation();
                    Context.Diagnostic.Errors.Add(new(ex));
                }
                finally
                {
                    Context.EndObject();
                }
            }
            else
            {
                var map = patternFields.Where(p => p.Key(Name)).Select(p => p.Value).FirstOrDefault();
                if (map != null)
                {
                    try
                    {
                        Context.StartObject(Name);
                        map(parentInstance, Name, Value);
                    }
                    catch (OverlayReaderException ex)
                    {
                        //TODO we're loosing the callstack here, it might be worth implementing an implicit converter or a derived class for the error
                        Context.Diagnostic.Errors.Add(new(ex.Pointer, ex.Message));
                    }
                    catch (OpenApiException ex)
                    {
                        ex.Pointer = Context.GetLocation();
                        Context.Diagnostic.Errors.Add(new(ex));
                    }
                    finally
                    {
                        Context.EndObject();
                    }
                }
                else if (!"$schema".Equals(Name, StringComparison.OrdinalIgnoreCase))
                {
                    Context.Diagnostic.Errors.Add(new OpenApiError("", $"{Name} is not a valid property at {Context.GetLocation()}"));
                }
            }
        }

        /// <inheritdoc/>
        public override JsonNode CreateAny()
        {
            throw new NotImplementedException();
        }
    }
}