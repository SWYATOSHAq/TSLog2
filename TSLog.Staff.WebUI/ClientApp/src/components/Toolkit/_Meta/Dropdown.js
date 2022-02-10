import React, { Component } from 'react';

export class Dropdown extends Component {
    static displayName = Dropdown.name;

    constructor(props) {
        super(props);
        this.state = {
            selectedValue: this.props.selectedValue,
        };
    }

    updateSelectedValue = (event) => {
        this.setState({ selectedValue: event.target.value })
        const callbackSelectValue = this.props.CallbackSelectValue;
        if (callbackSelectValue !== undefined) 
            callbackSelectValue(event.target.value);
        console.log(event.target.value);
    }

    render() {
        var data = this.props.DataList;
        if (data === undefined)
            data = [];

        var selectedVal = this.state.selectedValue;
        if (selectedVal === undefined)
            selectedVal = "";

            return (
            <div className="p-2">
                    <select className="form-select" aria-label="Default select example" value={selectedVal} onChange={this.updateSelectedValue}>
                    <option value="">Выберите значение</option>
                    {
                        data.map((row, i) => {
                            return (<option key={i} value={row.Id}>{row.DisplayName}</option>)
                        })
                    }
                </select>
            </div>
        );
    }
}