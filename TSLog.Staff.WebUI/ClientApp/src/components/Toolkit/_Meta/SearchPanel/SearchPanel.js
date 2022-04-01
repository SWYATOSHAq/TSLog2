import React, { Component } from 'react';
import './SearchPanel.css'

export class SearchPanel extends Component {
    static displayName = SearchPanel.name;

    render() {
        return <input className='form-control search-form' type='text' placeholder='поиск' />;
    }
}