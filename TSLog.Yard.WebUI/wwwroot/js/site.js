$(document).ready(function () {
    $(".datepicker").datepicker({
        format: "dd.mm.yyyy",
        autoclose: true,
        startDate: "0d",
        language: "ru",
        todayBtn: true,
        todayHighlight: true,
    })
        .datepicker("setDate", 'now');
});

function PrintCard() {
    window.print();
}

function Create() {
    $.post("/api/v1/YardOperationData", function () {



        alert("success");
    })
        .fail(function () {
            alert("error");
        })
}
