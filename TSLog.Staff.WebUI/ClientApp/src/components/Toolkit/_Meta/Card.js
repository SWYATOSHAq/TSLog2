import React, { Component } from 'react';
import { DropdownCompanies } from '../Dropdown/DropdownCompanies';
import { DropdownDepartments } from '../Dropdown/DropdownDepartments';
import { MultiselectWorkers } from '../Multiselect/MultiselectWorkers';
import { MultiselectPositions } from '../Multiselect/MultiselectPositions';

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

    handleChange(section, event) {


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
                                case "department":
                                    inputType = (<DropdownDepartments JsonSelectedValue={JSON.stringify(inputValue)} CallbackSelectValue={(newVal) => this.handleDropdownChange(newVal, curRowKey)} />);
                                    break;
                                case "employees":
                                    inputType = (<div className="col-12"><MultiselectWorkers /></div>);
                                    break;
                                case "positions":
                                    inputType = (<div className="col-12"><MultiselectPositions /></div>);
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
