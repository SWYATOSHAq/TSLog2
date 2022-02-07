import React, { Component } from 'react';

export class DeleteButton extends Component {
    static displayName = DeleteButton.name;

    constructor(props) {
        super(props);
    }

    render() {

        return (<button type="button" className="btn btn-outline-danger">Удалить</button>);
    }
}