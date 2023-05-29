import React, { Component } from 'react';
import { Dropdown } from '../_Meta/Dropdown';

export class DropdownGenders extends Component {
    static displayName = DropdownGenders.name;

    constructor(props) {
        super(props);
        this.state = {
            dropdownDataList: ['F', 'M'],
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