class ViewUtils {
    initDatePicker(id, date, onChangeFunction) {
        const datePicker = $("#" + id);
        if (datePicker == undefined || datePicker == null) {
            return;
        }
        datePicker.datepicker({
            format: "dd.mm.yyyy",
            autoclose: true,
            language: "ru",
            todayBtn: true,
            todayHighlight: true,
        });
        datePicker.datepicker("setDate", date);
        datePicker.on("changeDate", onChangeFunction);
    }
}

const viewUtils = new ViewUtils();