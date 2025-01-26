import React from 'react';

function Metrics({ data }) {
    return (
        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-6">
            {data.map((metric, index) => (
                <div
                    key={index}
                    className="bg-white shadow-lg rounded-lg p-4 flex flex-col justify-between"
                >
                    <div className="flex justify-between items-center">
                        <h2 className="text-sm font-medium text-gray-500">{metric.title}</h2>
                        <span className="text-gray-400 text-xl">
                            {metric.icon && metric.icon}
                        </span>
                    </div>
                    <div className="mt-4">
                        <p
                            className={`text-2xl font-bold ${
                                metric.value < 0 ? 'text-red-500' : 'text-green-500'
                            }`}
                        >
                            {metric.value}
                        </p>
                        {metric.subText && (
                            <p className="text-xs text-gray-400 mt-1">{metric.subText}</p>
                        )}
                    </div>
                </div>
            ))}
        </div>
    );
}

export default Metrics;
