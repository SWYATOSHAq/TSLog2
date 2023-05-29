import React, { Component } from 'react';
import { Dropdown } from '../_Meta/Dropdown';

export class DropdownWorkMode extends Component {
    static displayName = DropdownWorkMode.name;

    constructor(props) {
        super(props);
        this.state = {
            dropdownDataList: ['5/2-40', 'I', 'II', 'III', 'IV'],
        };
    }

    render() {
        return (
            <Dropdown
                DataList={this.state.dropdownDataList}
                selectedValue={this.props.JsonSelectedValue}
                CallbackSelectValue={this.props.CallbackSelectValue} />
        );
    }
}