using UnityEngine;
using System.Collections;

/*
 * This Character Controller assumes the following:
 *  - The Character game object (aka the character) it is attached to has:
 *      - A child game object indicating the position of the 'GroundCheck' point
 *      - A child game object indicating the position of the 'HeadCheck' point
 *      - An Animator that has the following parameters: Speed (float), Duck (bool)
 *        vspeed (float), Ground (bool) and hit (bool)
 *   
 * The character is moved horizontally by detecting movement on the horizontal input axis
 * and multiplying this value (between -1 and 1) by the horizontalSpeed the user sets via
 * the inspector. This calculated value will be a number between -HorizontalSpeed and
 * +HorizontalSpeed. We inturn use this value to calculate the desired velocity (velocity 
 * being speed in a particular direction - think arrows :-) ).
 * 
 * The character can only jump if on the ground. The way we determine whether or not the character
 * is on the ground is by getting all Collider2D object (if any), within a certain radius of 
 * a given point, that are attached to game object on a certain layer.  The point we use is the 
 * transform of the child 'GroundCheck' game object. The layer that the game objects have to be
 * on is stored in whatIsGround which is set via the inspector. We use the function 
 * Physics2D.OverlapCircle to get the Collider2D object.
 * 
 * It the player 'taps' the spacebar then the character will do a low jump. If the player holds
 * down the spacebar then the character will do a higher jump. The way we do this is by detecting
 * if the player is holding down the spacebar key. If they are not then we increase gravity so that 
 * the character is pulled to the ground faster than usual.
 * 
 */
public class CharacterController : MonoBehaviour
{
    // The speed at which we want the character to move up vertically when jumping.
    // The bigger the number the higher the jump.
    public float jumpSpeed;

    // How much to multiple gravitational force by when the character is falling. The
    // bigger the number the greater the gravitational pull and thus the quicker the fall.
    public float fallMultiplier = 5.0f;

    // How much to multiple gravitational force by when the character is falling from
    // a low jump.
    public float lowJumpMultiplier = 3.0f;

    // The horizontal speed the character will travel at.
    public float horizontalSpeed = 10;

    // The Layer that contains "ground" game objects.We will set this via the
    // inspector
    public LayerMask whatIsGround;

    // The transform (point) around which we will check, within a radius, whether there
    // are Collider2D objects on the 'whatIsGround' layer. This point will be at the
    // character feet and, as such, will be used to check if there is ground under her.
    public Transform groundcheck;

    // The transform (point) around which we will check, within a radius, whether there
    // are Collider2D objects on the 'whatIsGround' layer.  This point will be at the
    // character head and, as such, will be used to check if there is ground (e.g. platfrom) 
    // above her.
    public Transform headcheck;

    // The radius of the circle we are going to "draw" around the groundcheck Transform
    private float groundRadius = 0.5f;

    // The radius of the circle we are going to "draw" around the headcheck Transform
    private float headRadius = 0.3f;

    // grounded will be set to true while the character is on the ground
    private bool grounded;

    // platformOverHead will be set to true if the hero is grounded and there is 
    // platform over its head. We won't allow the character to 'get up from a
    // duck position' if this is true.
    public bool platformOverHead;

    // Bool to indicate has the jump button (spacebar) been pressed
    private bool jump;

    // Declare a variable which will store whether or not the "duck" button
    // has been pressed (se we can play the duck animation)
    private bool duck;

    // Create a variable to store whether we are facing right or not. I am going to
    // initialise it to true but you can change it in the inspector depending on which
    // direction you want your character to start in.
    bool facingRight = true;

    // The value of the Horizontal axis (some value between -1 and 1)
    private float hAxis;

    // A variable to store the RigidBody2D component attached to this game object
    private Rigidbody2D theRigidBody;

    // A variable to store the Animator component attached to this game object
    private Animator theAnimator;


    void Start()
    {
        // Set variables to a default state
        jump = false;
        grounded = false;
        duck = false;

        // Get the components we need
        theRigidBody = GetComponent<Rigidbody2D>();
        theAnimator = GetComponent<Animator>();

    }
    
    // Update is called once per frame
    void Update()
    {
        // Read the spacebar has been pressed down. Note that GetKeyDown will
        // return when the key (spacebar in this case) is pressed down but it
        // won't keep returning true while the key is being pressed
        jump = Input.GetKeyDown(KeyCode.Space);

        // Get the value of the Horizontal axis
        hAxis = Input.GetAxis("Horizontal");

        /*
          * hAxis will be a value between 1 and -1 (depending on whether you are going right or left). The animation
          * controller has a property called Speed which it uses to, for example, determine whether it should
          * transition to the Walk state. The Animator is expecting this Speed property to be equal to the value
          * of the horizontal axis so let's set it.
          */
        theAnimator.SetFloat("Speed", Mathf.Abs(hAxis));

        // Every frame i.e. everytime Unity calls Update, call the Physics2D.Overlap function
        // which takes three parameters:
        //  1. the position around which to "draw" the circle
        //  2. the radius of the circle
        //  3. the layer to check for overlaps in
        //
        // The function returns the Collider2D component (e.g. the BoxCollider2D component, or the
        // CircleCollider2D component, etc) of the game object the circle collides with. If it doesn't
        // collide with any game object then it returns null.
        Collider2D colliderWeCollidedWith = Physics2D.OverlapCircle(groundcheck.position, groundRadius, whatIsGround);

        /*
         * To convert one variable type to another we must "cast" it. In order to cast a variable we place
         * the type we want to cast it to infront of the variable name. I'm casting a variable of type
         * Collider2D to a variable of type bool, if the Collider2D variable contains a value (i.e. a Collider2D
         * object) then bool it is converted to will be true, otherwise it will be false. I store this 'converted'
         * value if the variable grounded.
         */
        grounded = (bool)colliderWeCollidedWith;

        // The Animator Controller attached to the Animator component has a property called Ground
        // which the Animator Controller uses to transition from one state to another. We must set
        // this Ground property to true when the Hero is on the ground and false otherwise.
        //
        // Because the Ground property on the Animator Controller is a boolean we need to use the
        // SetBool function to set it (see it in use below).

        theAnimator.SetBool("Ground", grounded);
        
        // The Animator has a vspeed parameter which should be set to the vertical (y) velocity of
        // the character. This is used by the Anumator in a blend tree to blend various 'falling'
        // animations depending on the velocity the character is falling at.

        // First the the y velocity of the character
        float yVelocity = theRigidBody.velocity.y;

        // Now use it to set the vspeed parameter
        theAnimator.SetFloat("vspeed", yVelocity);

      
        /* Finally we have a duck animation. Using Unity's Input class, get the value of the "Fire1" button.
         * 
         */
        duck = Input.GetButton("Fire1");

        if (duck)
        {
            // Okay, the player has pressed the 'Fire1' button, now set the Anoimators' Duck property to be true.
            theAnimator.SetBool("Duck", true);
        }
        else
        {
            /* Okay, the player is pressing the Fire1 (aka the duck) button. We want to avoid the following situation: 
               when the character has ducked down and walked under a platform and then the player leaves go the duck button
               and the character stands up and bangs it's head. To avoid this we need to do the following:
                - if the player isn't pressing the duck button
                - the character is on the ground
                - there is ground above the character e.g. a platfrom

               then don't allow the character to stand up i.e. keep the character in the duck position (even though the
               duck button isn't being pressed).
            */

            // Let's assume there is no platfrom over head
            platformOverHead = false;

            // We are only going to force to character into a duck if they are on the ground
            if (grounded)
            {
                // See if there are any Collider2D object on the whatIsGround layer within the headRadius
                // of the headcheck transfrom position
                platformOverHead = Physics2D.OverlapCircle(headcheck.position, headRadius, whatIsGround);
            }

            // platformOverHead at this stage will be either true or false. True if a platform was detected
            // overhead otherwise false. Use the variable to set the Duck parameter on the Animator i.e. set
            // the Duck parameter to either true or false
            theAnimator.SetBool("Duck", platformOverHead);
        }

        /*
         * If I am going right i.e. hAxis is greater than 0 but I am not facing right then flip me. Likewise 
         * for going left.
         *
         * I'm only going to flip if I'm grounded
         */

        if (grounded)
        {
            if ((hAxis > 0) && (facingRight == false))
            {
                Flip();
            }
            else if ((hAxis < 0) && (facingRight == true))
            {
                Flip();
            }
        }
       
    }

    /*
     * The FixedUpdate get called at fixed intervals by Unity at this is the function you use to apply
     * forces to your game objects as this function is used by Unity to keep the Physics system up-to-date.
     * You should try to keep the code within this function to a bare minimum as we don't want to slow down
     * the physics system.
     */
    void FixedUpdate()
    {
        // If not jumping then allow the character to be moved left or right
        if (grounded && !jump)
        {
            /* Set the velocity of the RigidBody component attached to the game object. We will keep the 
             * vertical velocity the same (it should be 0 anyway as the character is in the ground) but we will 
             * set the horizontal velocity to be equal to the horizontalSpeed * hAxis i.e. some value
             * between -horizontalSpeed and +horizontalSpeed
             */
            theRigidBody.velocity = new Vector2(horizontalSpeed * hAxis, theRigidBody.velocity.y);
        }
        else if (grounded && jump)
        {
            // Set the velocity, this time we keep the horizontal velocity the same but change the vertical (y)
            // velocity to jumpSpeed
            theRigidBody.velocity = new Vector2(theRigidBody.velocity.x, jumpSpeed);
        }

        
        if (theRigidBody.velocity.y < 0)
        {
            /* Okay, this means I'm falling down i.e. y velocity is less than 0. Notice below that I am not
             * setting the velocity to something new but rather I am adding to the existing velocity i.e. 
             * I am using +=
             * 
             * Vector2.up is the Vector (0,1)
             * Physics2D.gravity.y is, by default, set to -9.81
             * fallMultiplier is set via the inspector, defaulting to 5
             * Time.deltaTime is the number of seconds it took to complete the last frame, lets assume its  
             * 0.016 which is 1/60th of a second
             * 
             * so the value we are adding to the current velocity becomes:
             *          (0,1) * -9.81 * 5 * 0.016
             *      =>  (0,1) * -0.7848        
             *      =>  (0, -0.7848)
             *      
             * so we end up adding a slight downward (negative) velocity to the character and we do this 
             * every frame so the character falls down faster.
             */
            theRigidBody.velocity += Vector2.up * Physics2D.gravity.y * fallMultiplier * Time.deltaTime;

        }
        else if ((theRigidBody.velocity.y > 0) && (!Input.GetKey(KeyCode.Space)))
        {
            /* Okay, this means that the character is going up (y velocity is greater than 0) and the spacebar
             * is not being pressed i.e. a low jump.
             * 
             * This is similar to above except we are using lowJumpMultiplier.
             */
            theRigidBody.velocity += Vector2.up * Physics2D.gravity.y * lowJumpMultiplier * Time.deltaTime;
        }
        
    }

    /*
     * This is a new function that I wrote to flip the character in the x direction. The idea is simple. When 
     * the game starts make sure that the character is facing right (it asumes that the artist has drawn the
     * character and all it's animations facing right). We will keep track of whether the character is facing
     * right using the variable facingRight which will be either true or false (initially true). Anytime we
     * want to flip the character we toggle the value of the facingRight variable and flip the character. To 
     * flip the character we change the sign of it's x scale i.e. if it's x scale is 1 we change it to -1 and 
     * if it is -1 we change it to 1. To change the sign of any number simply multiply it by -1.
     */
    private void Flip()
    {
        //saying facingright is equal to not facingright (we are facing the opposite direction)
        facingRight = !facingRight;

        // Get the local scale. Local scale, similar to local position and rotation, is the scale of the
        // game object relative to it's parent. Sine this game object has no parent it's local scale is the
        // same as it's global scale

        // Every Unity script has access to a variable called transform which contains the Transform component
        // attached to this game object.
        
        Vector3 theScale = transform.localScale;

        //flip the x axis
        theScale.x *= -1;

        //apply it back to the local scale
        transform.localScale = theScale;  
    }
}
