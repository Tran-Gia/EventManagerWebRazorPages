
function AddModal(title, message) {
    let newModel = `
    <div class='modal fade' id='alertModal' tabindex='-1'>
        <div class='modal-dialog modal-dialog-centered'>
            <div class='modal-content'>
                <div class='modal-header'>
                    <h5 class='modal-title'>${title}</h5>
                    <button type='button' class='btn-close' onclick='RemoveModal()' aria-label='close'></button>
                </div>
                <div class='modal-body'>
                    ${message}
                </div>
                <div class='modal-footer'>
                    <button type='button' class= 'btn btn-primary' onclick='RemoveModal()'>Close</button>
                </div>
            </div>
        </div>
    </div>`;
    $("body").append(newModel);
    $("#alertModal").modal('show');

}
function RemoveModal() {
    $("#alertModal").modal('hide');
    $("#alertModal").remove();
}