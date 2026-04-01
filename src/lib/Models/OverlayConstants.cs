namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// Constants used in the Open API document.
/// </summary>
public static class OverlayConstants
{

    /// <summary>
    /// Field: ExtensionFieldNamePrefix
    /// </summary>
    public const string ExtensionFieldNamePrefix = "x-";

    /// <summary>
    /// Field: ReusableActionParameter.Name
    /// </summary>
    public const string ReusableActionParameterNameFieldName = "name";

    /// <summary>
    /// Field: ReusableActionParameter.Default
    /// </summary>
    public const string ReusableActionParameterDefaultFieldName = "default";
    /// <summary>
    /// Field: OverlayReusableAction.Parameters
    /// </summary>
    public const string ReusableActionParametersFieldName = "parameters";
    /// <summary>
    /// Field: OverlayReusableAction.EnvironmentVariables
    /// </summary>
    public const string ReusableActionEnvironmentVariablesFieldName = "environmentVariables";
    /// <summary>
    /// Field: OverlayReusableActionReference.$ref (serialized as extension for v1/v1.1 compatibility)
    /// </summary>
    public const string ReusableActionReferenceXReferenceFieldName = "x-$ref";
    /// <summary>
    /// Field: OverlayReusableActionReference.parameterValues (serialized as extension for v1/v1.1 compatibility)
    /// </summary>
    public const string ReusableActionReferenceXParameterValuesFieldName = "x-parameterValues";
    /// <summary>
    /// Prefix used to build OverlayReusableActionReference.Reference
    /// </summary>
    public const string ReusableActionReferencePrefix = "#/components/actions/";

    /// <summary>
    /// Field: OverlayAction.Target
    /// </summary>
    public const string ActionTargetFieldName = "target";
    /// <summary>
    /// Field: OverlayAction.Description
    /// </summary>
    public const string ActionDescriptionFieldName = "description";
    /// <summary>
    /// Field: OverlayAction.Remove
    /// </summary>
    public const string ActionRemoveFieldName = "remove";
    /// <summary>
    /// Field: OverlayAction.Update
    /// </summary>
    public const string ActionUpdateFieldName = "update";
    /// <summary>
    /// Field: OverlayAction.Copy (v1.0)
    /// </summary>
    public const string ActionXCopyFieldName = "x-copy";
    /// <summary>
    /// Field: OverlayAction.Copy (v1.1)
    /// </summary>
    public const string ActionCopyFieldName = "copy";

    /// <summary>
    /// Field: OverlayDocument.Overlay
    /// </summary>
    public const string DocumentOverlayFieldName = "overlay";
    /// <summary>
    /// Field: OverlayDocument.Info
    /// </summary>
    public const string DocumentInfoFieldName = "info";
    /// <summary>
    /// Field: OverlayDocument.Extends
    /// </summary>
    public const string DocumentExtendsFieldName = "extends";
    /// <summary>
    /// Field: OverlayDocument.Actions
    /// </summary>
    public const string DocumentActionsFieldName = "actions";

    /// <summary>
    /// Field: OverlayComponents.Actions
    /// </summary>
    public const string ComponentsActionsFieldName = "actions";

    /// <summary>
    /// Field: OverlayInfo.Title
    /// </summary>
    public const string InfoTitleFieldName = "title";
    /// <summary>
    /// Field: OverlayInfo.Version
    /// </summary>
    public const string InfoVersionFieldName = "version";
    /// <summary>
    /// Field: OverlayInfo.Description (v1.0)
    /// </summary>
    public const string InfoXDescriptionFieldName = "x-description";
    /// <summary>
    /// Field: OverlayInfo.Description (v1.1)
    /// </summary>
    public const string InfoDescriptionFieldName = "description";
}