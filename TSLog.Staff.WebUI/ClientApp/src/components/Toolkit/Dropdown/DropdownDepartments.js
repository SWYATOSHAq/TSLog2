import React, { Component } from 'react';
import { Dropdown } from '../_Meta/Dropdown';

export class DropdownDepartments extends Component {
    static displayName = DropdownDepartments.name;

    constructor(props) {
        super(props);
        this.state = {
            dropdownDataList: [],
        };
    }

    componentDidMount() {
        this.LoadData();
    }

    render() {
        return (
            <Dropdown
                DataList={this.state.dropdownDataList}
                selectedValue={this.props.JsonSelectedValue}
                CallbackSelectValue={this.props.CallbackSelectValue}/>
        );
    }

    async LoadData() {
        const response = await fetch('/api/v1/Staff/Department');
        const dataDepartments = await response.json();
        console.log(dataDepartments);

        this.setState({
            dropdownDataList: dataDepartments,
        });
    }
}