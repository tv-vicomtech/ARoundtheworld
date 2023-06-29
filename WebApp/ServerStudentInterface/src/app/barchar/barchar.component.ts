import { AfterViewInit, Component, ElementRef, ViewChild } from '@angular/core'
import { Chart, registerables } from 'chart.js'
import { Subject, takeUntil } from 'rxjs'
import { Student } from '../interfaces/student'
import { OrkestraService } from '../services/orkestra.service'

import { StudentService } from '../services/student.service'

Chart.register(...registerables)

@Component({
  selector: 'app-barchar',
  templateUrl: './barchar.component.html',
  styleUrls: [ './barchar.component.css' ]
})
export class BarcharComponent implements AfterViewInit {
  @ViewChild('chartCanvas1') private chartCanvas1!: ElementRef
  @ViewChild('chartCanvas2') private chartCanvas2!: ElementRef
  barChart1: any
  barChart2: any
  playerData: number[] = []
  observerData: number[] = []
  playerLabels: string[] = []
  observerLabels: string[] = []
  backgroundColors: string[] = []
  borderColors: string[] = []
  destroyed = new Subject<void>()

  students: Student[] = []

  constructor (private studentService: StudentService, private orkestraService: OrkestraService) {}

  ngAfterViewInit (): void {
    this.getBarChart1()
    this.getBarChart2()
    this.loadData()
  }

  ngOnInit (): void {
    this.getStudents()

    this.orkestraService.finishQuizSubject.subscribe(value => {
      if (value) {
        this.playerData = []
        this.observerData = []
        this.playerLabels = []
        this.observerLabels = []
        this.backgroundColors = []
        this.borderColors = []
        this.loadData()
      }
    })
  }

  getPlayerChartLabels () {
    return this.studentService.getStudentNameList('player')
  }

  getObserverChartLabels () {
    return this.studentService.getStudentNameList('observer')
  }

  getPlayerChartData () {
    return this.studentService.getStudentScoreList('player')
  }

  getObserverChartData () {
    return this.studentService.getStudentScoreList('observer')
  }

  getChartColors () {
    return this.studentService.getStudentColorList()
  }

  getStudents (): void {
    this.students = this.studentService.students

    this.studentService
      .getStudents()
      .pipe(takeUntil(this.destroyed))
      .subscribe((stu: Student[]) => {
        this.students = stu
        this.loadData()
      })
  }

  loadData () {
    this.reloadData()

    this.studentService.getChartSubject().subscribe(() => {
      this.reloadData()
    })
  }

  reloadData () {
    this.playerLabels = this.getPlayerChartLabels()
    this.observerLabels = this.getObserverChartLabels()

    this.playerData = this.getPlayerChartData()
    this.observerData = this.getObserverChartData()

    this.backgroundColors = this.getChartColors()
    this.borderColors = this.backgroundColors

    this.setPlotData()
  }

  setPlotData (): void {
    if (this.barChart1 && this.barChart1.data) {
      this.barChart1.data.labels = this.playerLabels
      // there is only 1 anyway
      this.barChart1.data.datasets.forEach(dataset => {
        dataset.data = this.playerData
      })
      // this.barChart.data.datasets[0].backgroundColors = this.backgroundColors
      // this.barChart.data.datasets[0].borderColors = this.borderColors
      this.barChart1.update()
    }

    if (this.barChart2 && this.barChart2.data) {
      this.barChart2.data.labels = this.observerLabels
      // there is only 1 anyway
      this.barChart2.data.datasets.forEach(dataset => {
        dataset.data = this.observerData
      })
      // this.barChart.data.datasets[0].backgroundColors = this.backgroundColors
      // this.barChart.data.datasets[0].borderColors = this.borderColors
      this.barChart2.update()
    }
  }

  getBarChart1 () {
    this.barChart1 = new Chart(this.chartCanvas1.nativeElement, {
      type: 'bar',
      data: {
        labels: this.playerLabels,
        datasets: [
          {
            label: '# Players',
            data: this.playerData,
            backgroundColor: this.backgroundColors,
            borderColor: this.backgroundColors
          }
        ]
      },
      options: {
        responsive: true,
        scales: {
          yAxes: {
            beginAtZero: true,
            title: {
              text: 'Score',
              display: true
            }
          }
        }
      }
    })
  }

  getBarChart2 () {
    this.barChart2 = new Chart(this.chartCanvas2.nativeElement, {
      type: 'bar',
      data: {
        labels: this.observerLabels,
        datasets: [
          {
            label: '# Observers',
            data: this.observerData,
            backgroundColor: this.backgroundColors,
            borderColor: this.backgroundColors
          }
        ]
      },
      options: {
        responsive: true,
        scales: {
          yAxes: {
            beginAtZero: true,
            title: {
              text: 'Score',
              display: true
            }
          }
        }
      }
    })
  }

  ngOnDestroy (): void {
    this.destroyed.next()
    this.destroyed.complete()
  }
}
