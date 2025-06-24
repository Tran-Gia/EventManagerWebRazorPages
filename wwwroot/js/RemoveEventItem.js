function RemoveEventItemConfirmationModal(baseElement, baseModel) {
    let element = $(baseElement).closest('.options');
    let elementId = element.attr('data-id');
    let model = `${baseModel}[${elementId}]`;
    let itemName = element.find(`input[name="${model}.ItemName"]`).val();
    let description = element.find(`textarea[name="${model}.Description"]`).val();
    description = (description.length >= 20) ? description.substring(0,22) + "..." : description;
    let amount = element.find(`input[name="${model}.Amount"]`).val();
    let newModel = `
    <div class='modal fade' id='alertModal' tabindex='-1'>
        <div class='modal-dialog modal-dialog-centered'>
            <div class='modal-content'>
                <div class='modal-header'>
                    <h5 class='modal-title'>Remove Event Item</h5>
                    <button type='button' class='btn-close' onclick='RemoveConfirmationModal()' aria-label='close'></button>
                </div>
                <div class='modal-body'>
                    Please confirm you want to remove this Event Item:
                    <div class='row'>
                        <label>Item Name: ${itemName}</label>
                    </div>
                    <div class='row'>
                        <label>Description: ${description}</label>
                    </div>
                    <div class='row'>
                        <label>Amount: ${amount}</label>
                    </div>
                </div>
                <div class='modal-footer'>
                    <button type='button' class= 'btn btn-primary' onclick="RemoveEventItem('${elementId}','${baseModel}')">Confirm</button>
                    <button type='button' class= 'btn btn-secondary' onclick='RemoveConfirmationModal()'>Close</button>
                </div>
            </div>
        </div>
    </div>`;
    $("body").append(newModel);
    $("#alertModal").modal('show');
}
function RemoveConfirmationModal() {
    $("#alertModal").modal('hide');
    $("#alertModal").remove();
}

function RemoveEventItem(elementId, baseModel) {
    RemoveConfirmationModal();
    var element = $(`.options[data-id='${elementId}']`);
    var model = `${baseModel}[${elementId}]`;
    element.fadeOut(500, function () {
        element.find(`input[name="${model}.EventId"]`).val('0');
        element.find(`input[name="${model}.Amount"]`).val('1');
        element.find(`input[name="${model}.ItemName"]`).val('unused item');
        element.find(`textarea[name="${model}.Description"]`).val('unused item');
        element.find('label[name="ItemNumber"]').attr('ishidden', 'true');
        UpdateItemNumber();
    });
}