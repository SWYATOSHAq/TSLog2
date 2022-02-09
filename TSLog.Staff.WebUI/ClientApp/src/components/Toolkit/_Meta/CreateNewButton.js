import React, { Component } from 'react';

export class CreateNewButton extends Component {
    static displayName = CreateNewButton.name;

    render() {
        return (<button type="button" className="btn btn btn-outline-success w-100" onClick={this.props.CreateNew}>Создать</button>);
    }
}