import { AfterViewInit, Component, ElementRef, Input, OnInit, ViewChild } from '@angular/core'
import { OrkestraService } from '../services/orkestra.service'
import * as THREE from 'three'
import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader.js'
import { DRACOLoader } from 'three/examples/jsm/loaders/DRACOLoader.js'
import {
  CameraTransform_Message,
  ObjectTransform_Message,
  PendingAnswer_Message
} from '../interfaces/messagesInterfaces'
import { GUI } from 'dat.gui'
import { applyCorrection, degreesToRadians } from '../utils/utils'

@Component({
  selector: 'app-earth',
  templateUrl: './earth.component.html',
  styleUrls: [ './earth.component.css' ]
})
export class EarthComponent implements OnInit, AfterViewInit {
  constructor (private orkestraService: OrkestraService) {}

  @ViewChild('earth_canvas') private canvasRef: ElementRef

  // Earth's properties
  @Input() public texturePath: string = '/assets/earth_colormap.jpg'
  @Input() public pinModelPath: string = '/assets/pinModel/scene.gltf'
  @Input() public earthModelPath: string = '/assets/earthModel/world.fbx'

  // Helper properties
  private camera!: THREE.PerspectiveCamera

  private get canvas (): HTMLCanvasElement {
    return this.canvasRef.nativeElement
  }

  private texture = new THREE.TextureLoader().load(this.texturePath)

  private geometry = new THREE.SphereGeometry(0.5, 128, 128)

  private earth: THREE.Mesh

  private pinModel: THREE.Object3D

  private renderer!: THREE.WebGLRenderer

  private scene!: THREE.Scene

  private earthFingerRotation: number = 0

  private cameraRotation: number = 0

  private earthRotOffsetX: number = Math.PI

  // private gui = new GUI()

  /**
   * Method taht creates the scene in which the earth is loaded
   */
  private createScene () {
    this.loadPinModel()

    // Scene
    this.scene = new THREE.Scene()
    this.scene.background = new THREE.Color(0x000000)

    // Lights
    const light = new THREE.AmbientLight(0xffffff)
    this.scene.add(light)

    // Camera
    const aspectRatio = this.getAspectRatio()
    this.camera = new THREE.PerspectiveCamera(
      40, // fov
      aspectRatio, // aspect ratio
      0.01, // near
      10 // far
    )
    this.camera.position.set(0, 0, -2.85)
    this.camera.rotation.set(0, 0, 0)
    this.camera.scale.set(1, 1, -2)

    // Earth
    this.texture.flipY = false
    this.earth = new THREE.Mesh(
      this.geometry,
      new THREE.MeshBasicMaterial({
        side: THREE.DoubleSide,
        map: this.texture,
        wireframe: false
      })
    )

    this.earth.position.set(0, 0, 0)
    this.earth.rotation.set(this.earthRotOffsetX, 0, 0)
    this.earth.scale.set(0.7, 0.7, 0.7)
    this.scene.add(this.earth)

    //------------------------------------------------------- DEBUG
    // console.log('---- this.earth', this.earth)

    // GUI
    // const earthFolder = this.gui.addFolder('Earth')
    // earthFolder.add(this.earth.position, 'x', -1, 1, 0.01).name('px').listen()
    // earthFolder.add(this.earth.position, 'y', -1, 1, 0.01).name('py').listen()
    // earthFolder.add(this.earth.position, 'z', -1, 1, 0.01).name('pz').listen()
    // earthFolder.add(this.earth.rotation, 'x', -10, 10, 0.0001).name('rx').listen()
    // earthFolder.add(this.earth.rotation, 'y', -10, 10, 0.0001).name('ry').listen()
    // earthFolder.add(this.earth.rotation, 'z', -10, 10, 0.0001).name('rz').listen()
    // earthFolder.open()

    // const cameraFolder = this.gui.addFolder('Camera')
    // cameraFolder.add(this.camera.position, 'x', -10, 10, 0.01).name('px').listen()
    // cameraFolder.add(this.camera.position, 'y', -10, 10, 0.01).name('py').listen()
    // cameraFolder.add(this.camera.position, 'z', -10, 10, 0.01).name('pz').listen()
    // cameraFolder.add(this.camera.rotation, 'x', -10, 10, 0.0001).name('rx').listen()
    // cameraFolder.add(this.camera.rotation, 'y', -10, 10, 0.0001).name('ry').listen()
    // cameraFolder.add(this.camera.rotation, 'z', -10, 10, 0.0001).name('rz').listen()
    // cameraFolder.open()

    //-------------------------------------------------------
  }

  private getAspectRatio () {
    return this.canvas.clientWidth / this.canvas.clientHeight
  }

  private animate () {
    requestAnimationFrame(this.animate.bind(this))
    this.setPinOrientation()
    this.renderer.render(this.scene, this.camera)
  }

  private startRenderingLoop () {
    // Configure the WebGL renderer with the canvas
    this.renderer = new THREE.WebGLRenderer({ canvas: this.canvas })
    this.renderer.setPixelRatio(devicePixelRatio)
    this.renderer.setSize(this.canvas.clientWidth, this.canvas.clientHeight)
    this.animate() // Start the render loop
  }

  private setPinOrientation (): void {
    // Make the pint look at the center of the earth
    if (this.earth) {
      this.earth.children.map(pin => {
        pin.lookAt(0, 0, 0)
      })
    }
  }

  private loadPinModel () {
    // Instantiate a loader
    const loader = new GLTFLoader()

    // Optional: Provide a DRACOLoader instance to decode compressed mesh data
    const dracoLoader = new DRACOLoader()
    dracoLoader.setDecoderPath('/examples/jsm/libs/draco/')
    loader.setDRACOLoader(dracoLoader)

    // Load a glTF resource
    loader.load(
      // resource URL
      this.pinModelPath,
      // called when the resource is loaded
      gltf => {
        this.pinModel = gltf.scene
      },
      progress => {
        // console.log(`${(progress.loaded / progress.total) * 100}% loaded`)
      },
      error => {
        console.log('An error happened while loading the pin model', error)
      }
    )
  }

  ngOnInit () {
    // Configure the listeners to all the events that will affect the scene

    this.orkestraService.earthTransformSubject.subscribe(value => {
      // console.log('orkestraService.earthTransformSubject', value)
      if (value) {
        const transform = value as ObjectTransform_Message
        // Apply the rotation to the earth converting from degrees to radians
        this.earth.rotation.set(
          this.earth.rotation.x,
          this.cameraRotation + -degreesToRadians(transform.rotY),
          this.earth.rotation.z
        )
        this.earthFingerRotation = -degreesToRadians(transform.rotY)
      }
    })

    this.orkestraService.cameraTransformSubject.subscribe(value => {
      // console.log('orkestraService.cameraTransformSubject', value)
      if (value) {
        // Apply the remote camera update to the threejs camera
        const transform = value as CameraTransform_Message
        // Apply the rotation to the camera converting from degrees to radians
        this.earth.rotation.set(
          this.earthRotOffsetX + -degreesToRadians(transform.rotX),
          this.earthFingerRotation + degreesToRadians(transform.rotY),
          this.earth.rotation.z
        )
        this.cameraRotation = degreesToRadians(transform.rotY)
      }
    })

    this.orkestraService.earthPinSubject.subscribe(value => {
      // console.log('orkestraService.earthPinSubject', value)
      if (value) {
        const transform = value as PendingAnswer_Message
        // Apply a correction on the phi angle
        const correctedCoords = applyCorrection([ transform.px, -transform.py, -transform.pz ])
        // const polarCoords = new THREE.Spherical()
        // polarCoords.setFromVector3(new THREE.Vector3(transform.px, -transform.py, -transform.pz))
        // const fixFactor = calculateFixFactor(polarCoords.phi)
        // polarCoords.phi += fixFactor
        // const correctedCoords = new THREE.Vector3()
        // correctedCoords.setFromSpherical(polarCoords)

        const pin = this.pinModel.clone()
        pin.scale.set(0.1, 0.1, 0.1)
        pin.position.set(correctedCoords.x, correctedCoords.y, correctedCoords.z)
        console.log('pin', pin)

        // Apply the rotation to the pin converting from degrees to radians
        // pin.rotation.set(
        //   degreesToRadians(transform.rx),
        //   -degreesToRadians(transform.ry),
        //   degreesToRadians(transform.rz)
        // )
        this.earth.add(pin)
      }
    })

    this.orkestraService.clearPinsSubject.subscribe(value => {
      // console.log('orkestraService.clearPinsSubject', value)
      if (value) {
        // Clear all pins from the earth
        this.earth.children.map(pin => {
          this.earth.remove(pin)
        })
      }
    })
  }

  ngAfterViewInit () {
    this.createScene()
    this.startRenderingLoop()
  }
}
function calculateFixFactor (phi: any) {
  throw new Error('Function not implemented.')
}
