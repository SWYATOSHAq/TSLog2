import React, { Component } from 'react';
import './BlockLevelButton.css'

export class BlockLevelButton extends Component {
    static displayName = BlockLevelButton.name;

    render() {

        return <button type="button" className="btn btn-primary btn-lg btn-block btn-block">Добавить</button>;
    }
}