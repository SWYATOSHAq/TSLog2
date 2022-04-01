import React, { Component } from 'react';



export class ItemStatusFilter extends Component {
    static displayName = ItemStatusFilter.name;

    render() {
         return (
             <div className="btn-group" role="group" aria-label="Basic example">
                <button type="button" className="btn btn-outline-secondary">Все</button>
                <button type="button" className="btn btn-outline-secondary">В штате</button>
                <button type="button" className="btn btn-outline-secondary">В подборе</button>
             </div>
        );
    };
};
