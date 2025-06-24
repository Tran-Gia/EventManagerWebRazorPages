
function AddLoadingSpinner(element) {
    let spinner = `
    <div class='spinner-border spinner-border-sm text-primary' role='status'>
        <span class='visually-hidden'>Loading...</span>
    </div>`;
    $(element).after(spinner);
}

function RemoveLoadingSpinner(element) {
    let spinner = $(element).next('.spinner-border');
    spinner.remove();
}