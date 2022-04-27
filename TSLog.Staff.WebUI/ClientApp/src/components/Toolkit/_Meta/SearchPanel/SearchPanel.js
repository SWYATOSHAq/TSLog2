import React, { Component } from 'react';
import './SearchPanel.css'

export class SearchPanel extends Component {
    static displayName = SearchPanel.name;

    state = {
        term: ''
    };

    onSearchChange = (e) => {
        const term = e.target.value;
        this.setState({ term });
        this.props.onSearchChange(term);
    }

    render() {
        return <input className='form-control search-form'
                      type='text'
                      placeholder='поиск'
                      value={this.state.term}
                      onChange={this.onSearchChange}
                />;
    }
}