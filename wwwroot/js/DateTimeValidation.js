$(function () {
    $("input[name='EventDetail.EventTime']").on("change", function (e) {
        DateTimeInputsValidation("EventTime", "Event Date");
    })
    $("input[name='EventDetail.RegistrationEndDate']").on("change", function (e) {
        DateTimeInputsValidation("RegistrationEndDate", "Registration End Date");
    })
    $("input[name='EventDetail.RegistrationStartDate']").on("change", function (e) {
        DateTimeInputsValidation("RegistrationStartDate", "Registration Start Date");
    })
    function DateTimeInputsValidation(elementName, elementMessageName) {
        let currentDate = new Date();
        let registrationStartDate = GetDateTimeFromInput('EventDetail.RegistrationStartDate');
        if (registrationStartDate > currentDate && DateTimeValidation('EventDetail.RegistrationStartDate', 'EventDetail.RegistrationEndDate', 'EventDetail.EventTime')) {
            $("input[type='submit']").prop('disabled', false);
            $("span[data-valmsg-for='EventDetail." + elementName + "']").text('');
        }
        else {
            $("span[data-valmsg-for='EventDetail." + elementName + "']").text('Invalid ' + elementMessageName);
            $("input[type='submit']").prop('disabled', true);
        }
    }

    function DateTimeValidation(elementName1, elementName2, elementName3) {
        let firstDate = GetDateTimeFromInput(elementName1);
        let secondDate = GetDateTimeFromInput(elementName2);
        let thirdDate = GetDateTimeFromInput(elementName3);
        if (secondDate < firstDate) {
            return false;
        }
        return !(secondDate >= thirdDate);
    }
    function GetDateTimeFromInput(elementName) {
        let rawDate = $("input[name='" + elementName + "']").val().split("-");
        if (rawDate != null)
            return new Date(rawDate[0], rawDate[1] - 1, rawDate[2]);
        return null;
    }
});
