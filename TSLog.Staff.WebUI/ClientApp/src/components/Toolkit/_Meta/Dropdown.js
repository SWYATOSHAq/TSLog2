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
        if (event.target.value !== "") {
            this.setState({ selectedValue: event.target.value })
            const callbackSelectValue = this.props.CallbackSelectValue;
            
            if (callbackSelectValue !== undefined)
                callbackSelectValue(event.target.value);
        }

    }

    render() {
        var data = this.props.DataList;
        if (data === undefined)
            data = [];

        var selectedVal = this.state.selectedValue;

        if (selectedVal === undefined)
            selectedVal = "";

        //selectedVal = '{"Id":"10000","Address":"","Info1":"","Info2":"","Code":"10000","Name":"ООО «БЫСТРО ГРУПП»","ShortName":"","AltName":"ООО «БЫСТРО ГРУПП»","DisplayName":"ООО «БЫСТРО ГРУПП»","Note":"","IsActive":true,"DisplayOrder":-1}';

        return (
            <select className="form-select" aria-label="Default select example" value={selectedVal} onChange={this.updateSelectedValue}>
                <option value="">Выберите значение</option>
                {
                    data.map((row, i) => {
                        return (<option key={i} value={JSON.stringify(row)}>{row.DisplayName}</option>)
                    })
                }
            </select>
        );
    }
}