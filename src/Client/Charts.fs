module Charts

open Feliz
open Feliz.Bulma
open Feliz.Recharts
open Shared

let lineChart (model : Model) =
    Recharts.lineChart [
        lineChart.width 500
        lineChart.height 300
        lineChart.data model.People
        lineChart.margin(top=5, right=30)
        lineChart.children [
            Recharts.cartesianGrid [ cartesianGrid.strokeDasharray(3, 3) ]
            Recharts.xAxis [ xAxis.dataKey (fun point -> point.First) ]
            Recharts.yAxis []
            Recharts.tooltip []
            Recharts.legend []
            Recharts.line [
                line.monotone
                line.dataKey (fun point -> point.Age)
                line.stroke "#8884d8"
            ]

            Recharts.line [
                line.monotone
                line.dataKey (fun point -> point.Height)
                line.stroke "#82ca9d"
            ]
        ]
    ]

let barChart (model : Model) =
    Recharts.barChart [
        barChart.width 500
        barChart.height 300
        barChart.data model.People
        barChart.margin(top=5, right=30)
        barChart.children [
            Recharts.cartesianGrid [ cartesianGrid.strokeDasharray(3, 3) ]
            Recharts.xAxis [ xAxis.dataKey (fun point -> point.First) ]
            Recharts.yAxis []
            Recharts.tooltip []
            Recharts.legend []
            Recharts.bar [
                bar.dataKey (fun point -> point.Age)
                bar.fill "#8884d8"
            ]

            Recharts.bar [
                bar.dataKey (fun point -> point.Height)
                bar.fill "#82ca9d"
            ]
        ]
    ]

let chartsView (model : Model) =
    if model.People.Length > 1 then
        Bulma.columns [
            Bulma.column [ lineChart model ]
            Bulma.column [ barChart model ]
        ]
    else
        Html.div []