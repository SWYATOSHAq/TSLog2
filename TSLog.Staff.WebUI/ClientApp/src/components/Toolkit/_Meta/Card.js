import React, { Component } from 'react';

export class Card extends Component {
    static displayName = Card.name;

    constructor(props) {
        super(props);
        this.state = {
            cardType: "",
            isNew: true,
            fieldList: {},
            fieldTypes: {},
            currentValues: {}
        };
    }

    handleChange(section, event) {


        this.setState(state => {
            const cardType = state.cardType;
            const isNew = state.isNew;
            const fieldList = state.fieldList;
            const fieldTypes = state.fieldTypes;

            const currentValues = state.currentValues;
            currentValues[section] = event.target.value;


            //this.state.currentValues[section] = event.target.value;
            return ({
                cardType, isNew, fieldList, fieldTypes, currentValues
            });
        });
    }


    render() {

        let cardState = this.state.isNew ? "(новая)" : "(редактирование)";

        return (
            <div className="col-6 m-2 p-2">
                {JSON.stringify(this.state.currentValues)}
                <div className="card">
                    <div className="card-header">
                      <h4>
                        {this.state.cardType} {cardState}
                      </h4>
                    </div>
                    <div className="card-body">

                        {Object.keys(this.state.fieldList).map((curRowKey, i) => {
                            let inputValue = "";
                            if (curRowKey in this.state.currentValues)
                                inputValue = this.state.currentValues[curRowKey];

                            let inputType = null;

                            switch (this.state.fieldTypes[curRowKey]) {
                                case "textarea":
                                    inputType = (<textarea rows="1" className="form-control" value={inputValue} onChange={(e) => this.handleChange(curRowKey, e)}></textarea>);
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
                        }) }

                        <div className="d-flex justify-content-center">
                            <button className="btn btn-success" type="button">сохранить</button>
                        </div>
                    </div>
                </div>
            </div>
        );
    }
}
