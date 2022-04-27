import React, { Component } from 'react';

export class ItemStatusFilter extends Component {
    static displayName = ItemStatusFilter.name;

    buttons = [
        { name: 'все', label: 'Все'},
        { name: 'в штате', label: 'В штате'},
        { name: 'в подборе', label: 'В подборе'}
    ];

    render() {
        const buttons = this.buttons.map(({name, label}) => {
            const isActive = this.props.filter === name;
            const clazz = isActive ? 'btn-secondary' : 'btn-outline-secondary';
            return (
                <button type="button"
                        className={`btn ${clazz}`}
                        key={ name }
                        onClick={() => this.props.onFilterChange(name)}>
                    { label }
                 </button>
            )
        });
        return (
            <div className="btn-group">{buttons}</div>
        )
    }
    }
//btn-outline-secondary
//role="group" aria-label="Basic example"