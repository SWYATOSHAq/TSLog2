import React, { Component } from 'react';
import { Dropdown } from '../_Meta/Dropdown';

export class DropdownCompanies extends Component {
    static displayName = DropdownCompanies.name;

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
                CallbackSelectValue={this.props.CallbackSelectValue} />
        );
    }

    async LoadData() {
        const response = await fetch('/api/v1/Staff/Employer');
        const dataEmployer = await response.json();
        console.log(dataEmployer);

        this.setState({
            dropdownDataList: dataEmployer,
        });
    }
}