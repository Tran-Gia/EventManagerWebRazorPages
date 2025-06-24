function SetDateTimePicker(inputDateElement, inputElement, spanElement) {

    var pickerInput = AddNewDateTimePicker(inputElement);
    var pickerSpan = AddNewDateTimePicker(spanElement);

    pickerInput.subscribe('change.td', (e) => {
        var rawDate = e.date;
        console.log('sth changed in input ' + inputElement + ": " + rawDate);
        SetDateTimeForHiddenField(inputDateElement, rawDate);
        if (!CheckEqualDateTimeFromDateTimePickers(pickerInput, pickerSpan)) {
            pickerSpan.dates.setValue(rawDate);
            console.log(pickerSpan.viewDate);
            console.log(pickerInput.viewDate);
        }
    });
    pickerSpan.subscribe('change.td', (e) => {
        var rawDate = e.date;
        console.log('sth changed in input ' + spanElement + ": " + rawDate);
        SetDateTimeForHiddenField(inputDateElement, rawDate);
        if (!CheckEqualDateTimeFromDateTimePickers(pickerInput, pickerSpan)) {
            pickerInput.dates.setValue(rawDate);
            console.log(pickerInput.viewDate);
            console.log(pickerSpan.viewDate);
        }
    });
    return { pickerInput, pickerSpan };
;
}

function SetLinkedDateTimePicker(picker1, picker2, dateAdjustment) {

    if (dateAdjustment == undefined || dateAdjustment == null)
        dateAdjustment = 0;

    picker1.pickerInput.subscribe('change.td', (e) => {
        let rawDate = e.date;
        rawDate.setDate(rawDate.getDate() + parseInt(dateAdjustment));

        picker2.pickerInput.updateOptions({
            restrictions: {
                minDate: rawDate
            }
        });
        picker2.pickerSpan.updateOptions({
            restrictions: {
                minDate: rawDate
            }
        });
    });

    picker2.pickerInput.subscribe('change.td', (e) => {
        let rawDate = e.date;
        rawDate.setDate(rawDate.getDate() - parseInt(dateAdjustment));

        picker1.pickerInput.updateOptions({
            restrictions: {
                maxDate: rawDate
            }
        });
        picker1.pickerSpan.updateOptions({
            restrictions: {
                maxDate: rawDate
            }
        });
    });
}

function CheckEqualDateTimeFromDateTimePickers(picker1, picker2) {
    var date1 = ConvertDateTimeForDateTimePicker(picker1.viewDate);
    var date2 = ConvertDateTimeForDateTimePicker(picker2.viewDate);
    console.log('date1:' + date1);
    console.log('date2:' + date2);
    console.log('isEqual:' + (date1==date2));
    return (date1 == date2);
}

function ConvertDateTimeForDateTimePicker(rawDate) {
    return rawDate.getFullYear() + "-" + (rawDate.getMonth() + 1) + "-" + rawDate.getDate();
}
function SetDateTimeForHiddenField(element, rawDate) {
    var valueDate = ConvertDateTimeForDateTimePicker(rawDate);
    $(`#${element}`).attr('value', valueDate).trigger('change');
}

function AddNewDateTimePicker(element) {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    const dateTimePicker = document.getElementById(element);
    var picker = new tempusDominus.TempusDominus(dateTimePicker, {
        display: {
            keepOpen: false,
            components: {
                clock: false,
                decades: false
            }
        },
        localization: {
            format: 'dd/MM/yyyy'
        },
        restrictions: {
            minDate: tomorrow
        },
        useCurrent: false,
        viewDate: tomorrow
    });
    return picker;
}