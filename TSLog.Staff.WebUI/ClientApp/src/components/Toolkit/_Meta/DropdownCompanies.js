import React, { Component } from 'react';

export class DropdownCompanies extends Component {
    static displayName = DropdownCompanies.name;

    render() {
        return (
            <div className="p-2">
                <select className="form-select" aria-label="Default select example">
                    <option selected>Open this select menu</option>
                    <option value="1">One</option>
                    <option value="2">Two</option>
                    <option value="3">Three</option>
                </select>
            </div>
        );
    }
}