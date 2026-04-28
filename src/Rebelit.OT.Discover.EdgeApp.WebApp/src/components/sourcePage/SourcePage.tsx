import { useNavigate } from 'react-router-dom'
import { useState, type ComponentProps } from 'react'
import Loginstyles from '../loginPage/LoginPage.module.css'
import type { SourceObject } from '../../models/SourceObject'


const defaultSourceObject: SourceObject = {
    sourceName: '',
    agentId: '',
}

function SourcePage() {

    const navigate = useNavigate()
    const [sourceObject, setSourceObject] = useState<SourceObject>(defaultSourceObject)
    const [isSubmitting, setIsSubmitting] = useState(false)
    const [sourceCreationSucceeded, SourceCreationSucceeded] = useState(false)
    const [errorMessage, setErrorMessage] = useState('')

    return (
        <div>
            
        </div>
    )
}

export default SourcePage