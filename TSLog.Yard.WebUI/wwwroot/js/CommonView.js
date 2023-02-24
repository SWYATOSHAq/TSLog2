/*
    Общий функционал для отображения.
    Автор - Буров Михаил (m.burov@tslog.ru).
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

    // Инициализирует check box
    initCheckbox(id, value, onChangeFunction) {
        const input = document.getElementById(id);
        if (input != undefined) {
            input.checked = value;
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
        return cell;
    }

    // Создаёт заголовок строки таблицы
    createRowHeader(row, styles, contents, cssText = "") {
        return this.createCellContent("th", row, styles, contents, cssText);
    }

    // Создаёт ячейку таблицы
    createCell(row, styles, contents, cssText = "") {
        return this.createCellContent("td", row, styles, contents, cssText);
    }

    // Создаёт ячейку таблицы c ЖИРНЫМ шрифтом
    createBoldCell(row, styles, contents, cssText = "") {
        const b = document.createElement("b");
        b.append(contents);
        return this.createCellContent("td", row, styles, [b], cssText);
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

    // "Нормализация" телефонного номера
    normalizePhoneNumber() {
        let phoneInputs = document.querySelectorAll('input[data-tel-input]');

        let getInputNumbersValue = function (input) {
            // Return stripped input value — just numbers
            return input.value.replace(/\D/g, '');
        }

        let onPhonePaste = function (e) {
            let input = e.target,
                inputNumbersValue = getInputNumbersValue(input);
            let pasted = e.clipboardData || window.clipboardData;
            if (pasted) {
                let pastedText = pasted.getData('Text');
                if (/\D/g.test(pastedText)) {
                    // Attempt to paste non-numeric symbol — remove all non-numeric symbols,
                    // formatting will be in onPhoneInput handler
                    input.value = inputNumbersValue;
                    return;
                }
            }
        }

        let onPhoneInput = function (e) {
            let input = e.target,
                inputNumbersValue = getInputNumbersValue(input),
                selectionStart = input.selectionStart,
                formattedInputValue = "";

            if (!inputNumbersValue) {
                return input.value = "";
            }

            if (input.value.length != selectionStart) {
                // Editing in the middle of input, not last symbol
                if (e.data && /\D/g.test(e.data)) {
                    // Attempt to input non-numeric symbol
                    input.value = inputNumbersValue;
                }
                return;
            }

            if (["7", "8", "9"].indexOf(inputNumbersValue[0]) > -1) {
                if (inputNumbersValue[0] == "9") inputNumbersValue = "7" + inputNumbersValue;
                let firstSymbols = (inputNumbersValue[0] == "8") ? "8" : "+7";
                formattedInputValue = input.value = firstSymbols + " ";
                if (inputNumbersValue.length > 1) {
                    formattedInputValue += '(' + inputNumbersValue.substring(1, 4);
                }
                if (inputNumbersValue.length >= 5) {
                    formattedInputValue += ') ' + inputNumbersValue.substring(4, 7);
                }
                if (inputNumbersValue.length >= 8) {
                    formattedInputValue += '-' + inputNumbersValue.substring(7, 9);
                }
                if (inputNumbersValue.length >= 10) {
                    formattedInputValue += '-' + inputNumbersValue.substring(9, 11);
                }
            } else {
                formattedInputValue = '+' + inputNumbersValue.substring(0, 16);
            }
            input.value = formattedInputValue;
        }
        let onPhoneKeyDown = function (e) {
            // Clear input after remove last symbol
            let inputValue = e.target.value.replace(/\D/g, '');
            if (e.keyCode == 8 && inputValue.length == 1) {
                e.target.value = "";
            }
        }
        for (let phoneInput of phoneInputs) {
            phoneInput.addEventListener('keydown', onPhoneKeyDown);
            phoneInput.addEventListener('input', onPhoneInput, false);
            phoneInput.addEventListener('paste', onPhonePaste, false);
        }
    }

    // Отображение числового значения с учётом его величины
    showNumeric(value) {
        return value.toString();
    }

    // Удаление элемента, если он существует
    removeElement(id, removeCondition, onClickEventHandler = null) {
        if (id === "") {
            return
        }
        const element = document.getElementById(id);
        if (element != undefined && element != null) {
            if (removeCondition) {
                element.remove();
            } else if (onClickEventHandler != null) {
                element.onclick = onClickEventHandler;
            }
        }
    }

    // Сортировка данных таблицы по указанной колонке
    sortTableByColumn(table, column, dataType = "text", asc = true) {
        const directionModifier = asc ? 1 : -1;
        const tBody = table.tBodies[0];
        const rows = Array.from(tBody.querySelectorAll("tr"));
        //sort each row
        const sortedRows = rows.sort((a, b) => {
            const aColText = a.querySelector(`td:nth-child(${column + 1})`).textContent.trim();
            const bColText = b.querySelector(`td:nth-child(${column + 1})`).textContent.trim();

            if (dataType === "float") {
                const aValue = parseFloat(aColText.replaceAll(" ", ""));
                const bValue = parseFloat(bColText.replaceAll(" ", ""));
                return aValue > bValue ? (1 * directionModifier) : (-1 * directionModifier);
            } else if (dataType === "integer") {
                const aValue = Number(aColText.replaceAll(" ", ""));
                const bValue = Number(bColText.replaceAll(" ", ""));
                return aValue > bValue ? (1 * directionModifier) : (-1 * directionModifier);
            }
            else {
                return aColText > bColText ? (1 * directionModifier) : (-1 * directionModifier);
            }
        });

        // Remove all existing TRs from the table
        while (tBody.firstChild) {
            tBody.removeChild(tBody.firstChild);
        }

        // Re-add the newly sorted rows
        tBody.append(...sortedRows);

        // Remember how the column is currently sorted
        table.querySelectorAll("th").forEach(th => th.classList.remove("th-sort-asc", "th-sort-desc"));
        table.querySelector(`th:nth-child(${column + 1})`).classList.toggle("th-sort-asc", asc);
        table.querySelector(`th:nth-child(${column + 1})`).classList.toggle("th-sort-desc", !asc);
    }

    // Формирует элемент фильтра по значению input-а для текста
    getFilterElementByStringInput(fieldName, input, condition = "like", options = { caseInsensitive: true, replaceOnE: true }) {
        let filter = {};
        filter.fieldName = fieldName;
        filter.condition = condition;
        let internalValue;
        if (options.caseInsensitive) {
            internalValue = input.value.toLowerCase();
        } else {
            internalValue = input.value;
        }
        if (options.replaceOnE) {
            internalValue = internalValue.replace("ё", "е");
        }
        filter.value = internalValue;
        return filter;
    }

    // Формирует элемент фильтра по значению input-а для числа
    getFilterElementByNumberInput(fieldName, input, condition = "=", options = { onlyPositive: true }) {
        let filter = {};
        filter.fieldName = fieldName;
        filter.condition = condition;
        if (options.onlyPositive) {
            if (input.value >= 0 )
                filter.value = input.value;
        }
        return filter;
    }

    // Формирует элемент фильтра по значению select-а
    getFilterElementBySelect(fieldName, select, condition = "=") {
        let filter = {};
        filter.fieldName = fieldName;
        filter.condition = condition;
        filter.value = select.value;
        return filter;
    }

    // Формирует элемент фильтра по значению date picker-а
    getFilterElementByDatePicker(fieldName, datePicker, condition = "=", options = {}) {
        let filter = {};
        filter.fieldName = fieldName;
        filter.condition = condition;
        filter.value = datePicker.value;

        if (options.includeTime) {
            filter.includeTime = true;
        }

        return filter;
    }

    // Сортировка таблицы
    sortTable() {
        document.querySelectorAll(".table-sortable th").forEach(headerCell => {
            headerCell.addEventListener("click", () => {
                const tableElement = headerCell.parentElement.parentElement.parentElement;
                const headerIndex = Array.prototype.indexOf.call(headerCell.parentElement.children, headerCell);
                const currentIsAscending = headerCell.classList.contains("th-sort-asc");

                let dataType = "text";
                if (headerCell.classList.contains("datatype-integer")) {
                    dataType = "integer";
                } else if (headerCell.classList.contains("datatype-float")) {
                    dataType = "float";
                }

                this.sortTableByColumn(tableElement, headerIndex, dataType, !currentIsAscending);
            });
        });
    }

    // Экспорт таблицы в excel (версия 1)
    tableToExcel_v1(table, fileName = "tableData", fileType = "xls") {
        const dataType = "application/vnd.ms-excel";
        const tableHtml = table.outerHTML.replace(/ /g, "%20");

        // Specify file name
        fileName = fileName ? fileName : "tableData";
        fileType = fileType ? fileType : "xlsx";
        fileName = fileName + "." + fileType;

        // Create download link element
        const downloadLink = document.createElement("a");

        document.body.appendChild(downloadLink);

        if (navigator.msSaveOrOpenBlob) {
            const blob = new Blob(["\ufeff", tableHtml], {
                type: dataType
            });
            navigator.msSaveOrOpenBlob(blob, fileName);
        } else {
            // Create a link to the file
            downloadLink.href = "data:" + dataType + ", " + tableHtml;

            // Setting the file name
            downloadLink.download = fileName;

            //triggering the function
            downloadLink.click();
        }
    }

    // Экспорт таблицы в excel (версия 2)
    tableToExcel_v2(table, fileName = "tableData", fileType = "xlsx", sheetName = "sheet1") {
        sheetName = sheetName ? sheetName : "sheet1"
        if (sheetName.length > 31) {
            sheetName = sheetName.substring(0, 31);
        }
        const sheetOptions = {
            sheet: sheetName
        }
        const file = XLSX.utils.table_to_book(table, sheetOptions);
        const options = {
            bookType: fileType,
            bookSST: true,
            type: 'base64'
        }
        XLSX.write(file, options);
        fileName = fileName ? fileName : "tableData";
        fileType = fileType ? fileType : "xlsx";
        fileName = fileName + "." + fileType;
        XLSX.writeFile(file, fileName);
    }
}

const viewUtils = new ViewUtils();