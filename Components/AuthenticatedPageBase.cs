using Microsoft.AspNetCore.Components;
using FrontBlazor_AppiGenericaCsharp.Services;

namespace FrontBlazor_AppiGenericaCsharp.Components
{
    public abstract class AuthenticatedPageBase : ComponentBase
    {
        [Inject] protected AuthService AuthService { get; set; } = default!;
        [Inject] protected NavigationManager Navigation { get; set; } = default!;

        protected bool isLoading = true;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var token = await AuthService.GetToken();
                if (string.IsNullOrEmpty(token))
                {
                    Navigation.NavigateTo("/login", true);
                }
                else
                {
                    isLoading = false;
                    StateHasChanged();
                }
            }
        }
    }
}