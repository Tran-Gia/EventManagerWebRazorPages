
function UpdateItemNumber() {
    var eventItemNumbers = Array.from(document.getElementsByName("ItemNumber"));
    var count = 0;
    eventItemNumbers.forEach(eventItemNumber => {
        if (!eventItemNumber.hasAttribute('ishidden')) {
            count++;
            eventItemNumber.innerText = count;
        }
    });
    $("small#ItemCounter").text(`${count} Event Items added`);
}