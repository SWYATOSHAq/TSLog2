import React, { Component } from 'react';
import { DropdownCompanies } from '../Dropdown/DropdownCompanies';
import { DropdownDepartments } from '../Dropdown/DropdownDepartments';
import { MultiselectWorkers } from '../Multiselect/MultiselectWorkers';
import { MultiselectPositions } from '../Multiselect/MultiselectPositions';
import { DropdownGenders } from '../Dropdown/DropdownGenders';
import { DropdownWorkMode } from '../Dropdown/DropdownWorkMode';

export class Card extends Component {
    static displayName = Card.name;

    constructor(props) {
        super(props);
        this.state = {
            cardType: "",
            isNew: true,
            fieldList: {},
            fieldTypes: {},
            currentValues: {},
            syncVal: (a) => { },
        };
    }

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

    handleChange(section, event) {

        this.normalizePhoneNumber();

        this.setState(state => {
            const cardType = state.cardType;
            const isNew = state.isNew;
            const fieldList = state.fieldList;
            const fieldTypes = state.fieldTypes;
            const syncVal = state.syncVal;

            const currentValues = state.currentValues;
            currentValues[section] = event.target.value;

            //syncVal(currentValues);

            //this.state.currentValues[section] = event.target.value;
            return ({
                cardType, isNew, fieldList, fieldTypes, currentValues, syncVal
            });
        });
    }


    handleDropdownChange(newValue, section) {
        this.setState(state => {
            const cardType = state.cardType;
            const isNew = state.isNew;
            const fieldList = state.fieldList;
            const fieldTypes = state.fieldTypes;
            const syncVal = state.syncVal;

            const currentValues = state.currentValues;
            currentValues[section] = JSON.parse(newValue);

            syncVal(currentValues);

            //this.state.currentValues[section] = event.target.value;
            return ({
                cardType, isNew, fieldList, fieldTypes, currentValues, syncVal
            });
        });
    }

    render() {

        let cardState = this.state.isNew ? "(новая)" : "(редактирование)";

        return (
            <div className="col m-2 p-2">

                <div className="card">
                    <div className="card-header">
                        <h4>
                            {this.state.cardType} {cardState}
                        </h4>
                    </div>
                    <div className="card-body">

                        {Object.keys(this.state.fieldList).map((curRowKey, i) => {
                            let inputValue = "";
                            if (this.state.currentValues !== undefined && curRowKey in this.state.currentValues)
                                inputValue = this.state.currentValues[curRowKey];

                            let inputType = null;

                            switch (this.state.fieldTypes[curRowKey]) {
                                case "textarea":
                                    inputType = (<textarea rows="1" className="form-control" value={inputValue} onChange={(e) => this.handleChange(curRowKey, e)}></textarea>);
                                    break;
                                case "datetime":
                                    if (this.state.cardType === "Работник") {
                                        inputType = (<input type="date" className="form-control" value={inputValue} onChange={(e) => this.handleChange(curRowKey, e)}></input>);
                                    } else {
                                        inputType = (<input type="datetime-local" className="form-control" value={inputValue} onChange={(e) => this.handleChange(curRowKey, e)}></input>);
                                    }
                                    break;
                                case "date":
                                    if (this.state.cardType === "Работник") {
                                        inputType = (<input type="date" className="form-control" value={inputValue} onChange={(e) => this.handleChange(curRowKey, e)}></input>);
                                    } else {
                                        inputType = (<input type="datetime-local" className="form-control" value={inputValue} onChange={(e) => this.handleChange(curRowKey, e)}></input>);
                                    }
                                    break;
                                case "bool":
                                    inputType = (<input type="checkbox" className="form-check-input m-2" checked={inputValue} onChange={(e) => this.handleChange(curRowKey, e)}></input>);
                                    break;
                                case "company":
                                    inputType = (<DropdownCompanies JsonSelectedValue={JSON.stringify(inputValue)} CallbackSelectValue={(newVal) => this.handleDropdownChange(newVal, curRowKey)} />);
                                    break;
                                case "gender":
                                    inputType = (<DropdownGenders JsonSelectedValue={JSON.stringify(inputValue)} CallbackSelectValue={(newVal) => this.handleDropdownChange(newVal, curRowKey)} />);
                                    break;
                                case "workMode":
                                    inputType = (<DropdownWorkMode JsonSelectedValue={JSON.stringify(inputValue)} CallbackSelectValue={(newVal) => this.handleDropdownChange(newVal, curRowKey)} />);
                                    break;
                                case "department":
                                    inputType = (<DropdownDepartments JsonSelectedValue={JSON.stringify(inputValue)} CallbackSelectValue={(newVal) => this.handleDropdownChange(newVal, curRowKey)} />);
                                    break;
                                case "employees":
                                    inputType = (<div className="col-12"><MultiselectWorkers /></div>);
                                    break;
                                case "positions":
                                    inputType = (<div className="col-12"><MultiselectPositions /></div>);
                                    break;
                                case "email":
                                    inputType = (<input type="email" className="form-control" value={inputValue} onChange={(e) => this.handleChange(curRowKey, e)} required></input>);
                                    break;
                                case "tel":
                                    inputType = (<input type="tel" data-tel-input className="form-control" value={inputValue} onChange={(e) => this.handleChange(curRowKey, e)}></input>);
                                    break;

                                default:
                                    inputType = (<input className="form-control" value={inputValue} onChange={(e) => this.handleChange(curRowKey, e)}></input>);
                                    break;

                            }

                            return (
                                <div className="input-group m-2" key={i}>
                                    <div className="input-group-prepend col-4">
                                        <span className="input-group-text">{this.state.fieldList[curRowKey]}</span>
                                    </div>
                                    {inputType}
                                </div>
                            )
                        })}

                        <div className="d-flex justify-content-center">
                            <button className="btn btn-success" type="button" onClick={this.state.syncVal.bind(this, this.state.currentValues)}>сохранить</button>
                        </div>
                    </div>
                </div>
            </div>
        );
    }
}
