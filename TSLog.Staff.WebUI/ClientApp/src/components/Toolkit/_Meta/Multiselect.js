import React, { Component } from 'react';

export class Multiselect extends Component {
    static displayName = Multiselect.name;

    render() {
        return (
            <div className="p-2">
                <select className="form-select" multiple aria-label="multiple select example">
                    <option selected>Open this select menu</option>
                    <option value="1">One</option>
                    <option value="2">Two</option>
                    <option value="3">Three</option>
                </select>
            </div>
        );
    }
}