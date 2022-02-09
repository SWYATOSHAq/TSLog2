import React, { Component } from 'react';

export class ChangeButton extends Component {
    static displayName = ChangeButton.name;

    render() {

        return (<button type="button" className="btn btn-outline-primary" onClick={this.props.SelectRow.bind(this, this.props.rowIndex) }>Изменить</button>);
    }
}