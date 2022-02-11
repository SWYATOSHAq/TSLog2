import React, { Component } from 'react';
import { Multiselect } from '../_Meta/Multiselect';

export class MultiselectWorkers extends Component {
    static displayName = MultiselectWorkers.name;

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
            <Multiselect
                DataList={this.state.dropdownDataList}
                DataValues={this.props.DataValues}
                callbackOnChange={this.props.callbackOnChange}
                key={this.state.dropdownDataList}
            />
        );
    }

    async LoadData() {
        const response = await fetch('/api/v1/Staff/Employee');
        const dataEmployee = await response.json();
        console.log(dataEmployee);

        this.setState({
            dropdownDataList: dataEmployee,
        });
    }
}