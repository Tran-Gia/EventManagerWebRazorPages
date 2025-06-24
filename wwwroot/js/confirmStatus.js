"use strict";

let connection = new signalR.HubConnectionBuilder().withUrl("/confirmStatusHub").build();

connection.start().then(function () {
    
}).catch(function (err) {
    return console.error(err.toString());
});

connection.on("ReceiveUserUpdateConfirmStatus", function (userRegisId, confirmStatus) {
    console.log(userRegisId);
    console.log(confirmStatus);
    let status = "";
    switch (confirmStatus) {
        case -1: status = "Denied"; break;
        case 1: status = "Confirmed"; break;
        case 0: default: status = "Pending"; break;
    }
    $("label[name='confirmStatus'][data-id='" + userRegisId + "']").text(status +'');
    if (status == "Confirmed") {
        $("[name='cancelCheckinButton'][data-id='" + userRegisId + "']").prop('disabled', true);
    }
});