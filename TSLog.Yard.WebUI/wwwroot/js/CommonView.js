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

    initSelect(id, value, onChangeFunction) {
        const select = document.getElementById(id);
        if (select != undefined) {
            select.value = value;
            select.onchange = onChangeFunction;
        }
    }

    initInput(id, value, onChangeFunction) {
        const input = document.getElementById(id);
        if (input != undefined) {
            input.value = value;
            input.onchange = onChangeFunction;
        }
    }

    createCellContent(tagName, row, styles, contents, cssText = "") {
        const cell = document.createElement(tagName);
        for (let style of styles) {
            cell.classList.add(style);
        }
        for (let content of contents) {
            cell.append(content);
        }
        cell.style.cssText = cssText;
        row.append(cell);
    }

    createRowHeader(row, styles, contents, cssText = "") {
        this.createCellContent("th", row, styles, contents, cssText);
    }

    createCell(row, styles, contents, cssText = "") {
        this.createCellContent("td", row, styles, contents, cssText);
    }
}

const viewUtils = new ViewUtils();