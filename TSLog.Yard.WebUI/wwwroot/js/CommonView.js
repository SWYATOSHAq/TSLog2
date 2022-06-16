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
}

const viewUtils = new ViewUtils();