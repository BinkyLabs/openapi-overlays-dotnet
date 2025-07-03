// Copyright (c) Microsoft Corporation. All rights reserved.
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

        public string Name { get; set; }

        public ParseNode Value { get; set; }

        public void ParseField<T>(
            T parentInstance,
            Dictionary<string, Action<T, ParseNode, OverlayDocument>> fixedFields,
            Dictionary<Func<string, bool>, Action<T, string, ParseNode, OverlayDocument>> patternFields,
            OverlayDocument hostDocument)
        {
            if (fixedFields.TryGetValue(Name, out var fixedFieldMap))
            {
                try
                {
                    Context.StartObject(Name);
                    fixedFieldMap(parentInstance, Value, hostDocument);
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
                        map(parentInstance, Name, Value, hostDocument);
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
                    var error = new OpenApiError("", $"{Name} is not a valid property at {Context.GetLocation()}");
                    if ("$schema".Equals(Name, StringComparison.OrdinalIgnoreCase))
                    {
                        Context.Diagnostic.Warnings.Add(error);
                    }
                    else
                    {
                        Context.Diagnostic.Errors.Add(error);
                    }
                }
            }
        }

        public override JsonNode CreateAny()
        {
            throw new NotImplementedException();
        }
    }
}