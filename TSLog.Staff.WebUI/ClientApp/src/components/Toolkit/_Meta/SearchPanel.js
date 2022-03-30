import React, { Component } from 'react';

export class SearchPanel extends Component {
    static displayName = SearchPanel.name;

    render() {
        return <input placeholder='поиск' />;
    }
}