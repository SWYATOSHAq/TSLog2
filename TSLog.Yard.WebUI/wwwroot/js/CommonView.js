/*
    Общий функционал для отображения.
    Автор - Буров Михаил.
*/

// Класс, реализующий дополнительный и вспомогательный функционал (для пользовательского интерфейса)
class ViewUtils {
    // Инициализирует элемент управления для выбора даты и времени
    initDatePicker(id, date, onChangeFunction) {
        const dateInput = $("#" + id);
        if (dateInput == undefined || dateInput == null) {
            return;
        }
        dateInput.datepicker({
            format: "dd.mm.yyyy",
            autoclose: true,
            language: "ru",
            todayBtn: true,
            todayHighlight: true,
        });
        dateInput.datepicker("setDate", date);
        dateInput.on("changeDate", onChangeFunction);
    }

    // Инициализирует выпадающий список
    initSelect(id, value, onChangeFunction) {
        const select = document.getElementById(id);
        if (select != undefined) {
            select.value = value;
            select.onchange = onChangeFunction;
        }
    }

    // Инициализирует окно ввода
    initInput(id, value, onChangeFunction) {
        const input = document.getElementById(id);
        if (input != undefined) {
            input.value = value;
            input.onchange = onChangeFunction;
        }
    }

    // Создаёт содержимое ячейки таблицы
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

    // Создаёт заголовок строки таблицы
    createRowHeader(row, styles, contents, cssText = "") {
        this.createCellContent("th", row, styles, contents, cssText);
    }

    // Создаёт ячейку таблицы
    createCell(row, styles, contents, cssText = "") {
        this.createCellContent("td", row, styles, contents, cssText);
    }

    // Показывает сообщение об ошибке
    showError(message, showLog = true, showAlert = true) {
        if (showLog) {
            console.log(message);
        }
        if (showAlert) {
            alert(message);
        }
    }
}

const viewUtils = new ViewUtils();