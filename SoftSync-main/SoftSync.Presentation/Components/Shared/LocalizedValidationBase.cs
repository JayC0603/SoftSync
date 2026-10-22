using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace SoftSync.Presentation.Components.Shared;

public abstract class LocalizedValidationBase : LocalizedComponentBase
{
    [CascadingParameter] protected EditContext? EditContext { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    private EditContext? subscribed;
    protected override void OnParametersSet()
    {
        if (subscribed == EditContext) return;
        if (subscribed is not null) subscribed.OnValidationStateChanged -= ValidationChanged;
        subscribed = EditContext ?? throw new InvalidOperationException("Validation must be inside an EditForm.");
        subscribed.OnValidationStateChanged += ValidationChanged;
    }
    private void ValidationChanged(object? sender, ValidationStateChangedEventArgs args) => _ = InvokeAsync(StateHasChanged);
    public override void Dispose()
    {
        if (subscribed is not null) subscribed.OnValidationStateChanged -= ValidationChanged;
        base.Dispose();
    }
}
